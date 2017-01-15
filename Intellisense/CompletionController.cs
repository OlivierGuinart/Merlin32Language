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

            Func<CommandFilter> createCommandHandler = delegate { return new CommandFilter(textViewAdapter, textView, this); };
            textView.Properties.GetOrCreateSingletonProperty(createCommandHandler);
        }
    }

    internal sealed class CommandFilter : IOleCommandTarget
    {
        private IOleCommandTarget _nextCommandHandler;
        private ITextView _textView;
        private VsTextViewCreationListener _provider;
        private ICompletionSession _session;

        internal CommandFilter(IVsTextView textViewAdapter, ITextView textView, VsTextViewCreationListener provider)
        {
            _textView = textView;
            _provider = provider;

            //add the command to the command chain
            textViewAdapter.AddCommandFilter(this, out _nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(_provider.ServiceProvider))
            {
                return _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
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
                if (_session != null && !_session.IsDismissed)
                {
                    //if the selection is fully selected, commit the current session 
                    if (_session.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        _session.Commit();
                        //also, don't add the character to the buffer 
                        return VSConstants.S_OK;
                    }
                    else
                    {
                        //if there is no selection, dismiss the session
                        _session.Dismiss();
                    }
                }
            }

            //pass along the command so the char is added to the buffer 
            int retVal = _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            bool handled = false;
            // Test for '-' is to catch ELUP (--^)
            if (!typedChar.Equals(char.MinValue) && ((char.IsLetterOrDigit(typedChar)) || ((typedChar == '\'') || (typedChar == '"') || (typedChar == '-'))))
            {
                if (_session == null || _session.IsDismissed) // If there is no active session, bring up completion
                {
                    TriggerCompletion();
                    // No need to filter for single and double-quotes, the choice IS the characted, just doubled, and already populated in a single completionset if we're here...
                    if ((typedChar == '\'') || (typedChar == '"'))
                    {
                        // We need to save the currect caret position because we'll position it in between the single/double quotes after the commit...
                        ITextCaret caretBeforeCommit = _session.TextView.Caret;
                        _session.Commit();
                        _textView.Caret.MoveTo(caretBeforeCommit.Position.BufferPosition - 1);
                    }
                    else if (!_session.IsDismissed)
                    {
                        _session.Filter();
                    }
                }
                else     //the completion session is already active, so just filter
                {
                    _session.Filter();
                }
                handled = true;
            }
            else if (commandID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE   //redo the filter if there is a deletion
                || commandID == (uint)VSConstants.VSStd2KCmdID.DELETE)
            {
                if (_session != null && !_session.IsDismissed)
                    _session.Filter();
                handled = true;
            }

            if (handled) return VSConstants.S_OK;
            return retVal;
        }

        private bool TriggerCompletion()
        {
            //the caret must be in a non-projection location 
            SnapshotPoint? caretPoint =
            _textView.Caret.Position.Point.GetPoint(
            textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);
            if (!caretPoint.HasValue)
            {
                return false;
            }

            _session = _provider.CompletionBroker.CreateCompletionSession(_textView,
                caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
                true);

            // We need to check now whether we are in a comment or not, because if we are, we don't want to provide a completion list to the user
            ITextSnapshot snapshot = caretPoint.Value.Snapshot;
            var triggerPoint = (SnapshotPoint)_session.GetTriggerPoint(snapshot);
            var snapshotSpan = new SnapshotSpan(triggerPoint, 0);
            foreach (VSMerlin32.Coloring.Data.SnapshotHelper item in VSMerlin32.Coloring.Merlin32CodeHelper.GetTokens(snapshotSpan))
            {
                if (item.Snapshot.IntersectsWith(snapshotSpan))
                {
                    if (item.TokenType == Merlin32TokenTypes.Merlin32Comment)
                    {
                        _session.Dismiss();
                        break;
                    }
                }
            }

            if (!_session.IsDismissed)
            {
                //subscribe to the Dismissed event on the session 
                _session.Dismissed += OnSessionDismissed;
                _session.Start();
            }

            return true;
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            _session.Dismissed -= OnSessionDismissed;
            _session = null;
        }
    }

    #endregion
}