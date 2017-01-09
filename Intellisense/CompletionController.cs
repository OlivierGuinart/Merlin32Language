using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;

using Microsoft.VisualStudio.Shell;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

using Microsoft.VisualStudio.Text.Operations;


namespace VSMerlin32
{
    #region Command Filter

    [Export(typeof(IVsTextViewCreationListener))]
    [Name("Merlin32CompletionController")]
    [ContentType("Merlin32")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class VsTextViewCreationListener : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdaptersFactory = null;
        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }
        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdaptersFactory.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            Func<CommandFilter> createCommandHandler = delegate() { return new CommandFilter(textViewAdapter, textView, this); };
            textView.Properties.GetOrCreateSingletonProperty(createCommandHandler);
        }
    }

    internal sealed class CommandFilter : IOleCommandTarget
    {
        private IOleCommandTarget m_nextCommandHandler;
        private ITextView m_textView;
        private VsTextViewCreationListener m_provider;
        private ICompletionSession m_session;

        internal CommandFilter(IVsTextView textViewAdapter, ITextView textView, VsTextViewCreationListener provider)
        {
            this.m_textView = textView;
            this.m_provider = provider;

            //add the command to the command chain
            textViewAdapter.AddCommandFilter(this, out m_nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return m_nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(m_provider.ServiceProvider))
            {
                return m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }
            //make a copy of this so we can look at it after forwarding some commands 
            uint commandID = nCmdID;
            char typedChar = char.MinValue;
            //make sure the input is a char before getting it 
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            //check for a commit character 
            if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN
                || nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB
                || char.IsWhiteSpace(typedChar) 
                || char.IsPunctuation(typedChar))
            {
                //check for a a selection 
                if (m_session != null && !m_session.IsDismissed)
                {
                    //if the selection is fully selected, commit the current session 
                    if (m_session.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        m_session.Commit();
                        //also, don't add the character to the buffer 
                        return VSConstants.S_OK;
                    }
                    else
                    {
                        //if there is no selection, dismiss the session
                        m_session.Dismiss();
                    }
                }
            }

            //pass along the command so the char is added to the buffer 
            int retVal = m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            bool handled = false;
            if (!typedChar.Equals(char.MinValue) && ((char.IsLetterOrDigit(typedChar)) || ((typedChar == '\'') || (typedChar == '"'))))
            {
                if (m_session == null || m_session.IsDismissed) // If there is no active session, bring up completion
                {
                    this.TriggerCompletion();
                    // No need to filter for single and double-quotes, the choice IS the characted, just doubled, and already populated in a single completionset if we're here...
                    if ((typedChar == '\'') || (typedChar == '"'))
                    {
                        // We need to save the currect caret position because we'll position it in between the single/double quotes after the commit...
                        ITextCaret CaretBeforeCommit = m_session.TextView.Caret;
                        m_session.Commit();
                        this.m_textView.Caret.MoveTo(CaretBeforeCommit.Position.BufferPosition - 1);
                    }
                    else if (!m_session.IsDismissed)
                    {
                        m_session.Filter();
                    }
                }
                else     //the completion session is already active, so just filter
                {
                    m_session.Filter();
                }
                handled = true;
            }
            else if (commandID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE   //redo the filter if there is a deletion
                || commandID == (uint)VSConstants.VSStd2KCmdID.DELETE)
            {
                if (m_session != null && !m_session.IsDismissed)
                    m_session.Filter();
                handled = true;
            }

            if (handled) return VSConstants.S_OK;
            return retVal;
        }

        private bool TriggerCompletion()
        {
            //the caret must be in a non-projection location 
            SnapshotPoint? caretPoint =
            m_textView.Caret.Position.Point.GetPoint(
            textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);
            if (!caretPoint.HasValue)
            {
                return false;
            }

            m_session = m_provider.CompletionBroker.CreateCompletionSession(m_textView,
                caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
                true);

            // We need to check now whether we are in a comment or not, because if we are, we don't want to provide a completion list to the user
            ITextSnapshot snapshot = caretPoint.Value.Snapshot;
            var triggerPoint = (SnapshotPoint)m_session.GetTriggerPoint(snapshot);
            var snapshotSpan = new SnapshotSpan(triggerPoint, 0);
            foreach (VSMerlin32.Coloring.Data.SnapshotHelper item in VSMerlin32.Coloring.Merlin32CodeHelper.GetTokens(snapshotSpan))
            {
                if (item.Snapshot.IntersectsWith(snapshotSpan))
                {
                    if (item.TokenType == Merlin32TokenTypes.Merlin32Comment)
                    {
                        m_session.Dismiss();
                        break;
                    }
                }
            }

            if (!m_session.IsDismissed)
            {
                //subscribe to the Dismissed event on the session 
                m_session.Dismissed += this.OnSessionDismissed;
                m_session.Start();
            }

            return true;
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            m_session.Dismissed -= this.OnSessionDismissed;
            m_session = null;
        }
    }

    #endregion
}