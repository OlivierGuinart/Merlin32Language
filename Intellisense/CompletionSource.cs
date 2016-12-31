using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace VSMerlin32
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("Merlin32")]
    [Name("Merlin32Completion")]
    internal class Merlin32CompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new Merlin32CompletionSource(this, textBuffer);
        }
    }

    internal class Merlin32CompletionSource : ICompletionSource
    {
        private Merlin32CompletionSourceProvider m_sourceprovider;
        private ITextBuffer m_buffer;
        private List<Completion> m_compList;
        private bool m_isDisposed = false;

        public Merlin32CompletionSource(Merlin32CompletionSourceProvider sourceprovider, ITextBuffer buffer)
        {
            m_sourceprovider = sourceprovider;
            m_buffer = buffer;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            if (m_isDisposed)
                throw new ObjectDisposedException("Merlin32CompletionSource");

            List<string> strList = new List<string>();
            char chTyped = ((session.TextView.Caret.Position.BufferPosition) - 1).GetChar();
            // Testing for single and double quotes because these will be autocompleted...
            if ((chTyped == '\'') || (chTyped == '"'))
            {
                strList.Add(chTyped.ToString() + chTyped.ToString());
            }
            else
            {
                // If the user has been typing lowercase, we'll present a lowercase list of keywords/opcodes...
                if (char.IsLower(chTyped))
                {
                    foreach (Merlin32Opcodes token in Enum.GetValues(typeof(Merlin32Opcodes)))
                    {
                        strList.Add(token.ToString().ToLower());
                    }
                    foreach (Merlin32Directives token in Enum.GetValues(typeof(Merlin32Directives)))
                    {
                        if (token.ToString().ToLower() == Merlin32Directives.ELUP.ToString().ToLower())
                        {
                            strList.Add(Resources.directives.ELUPValue);
                        }
                        else
                        {
                            strList.Add(token.ToString().ToLower());
                        }
                    }
                    foreach (Merlin32DataDefines token in Enum.GetValues(typeof(Merlin32DataDefines)))
                    {
                        strList.Add(token.ToString().ToLower());
                    }
                }
                else
                {
                    foreach (Merlin32Opcodes token in Enum.GetValues(typeof(Merlin32Opcodes)))
                    {
                        strList.Add(token.ToString());
                    }
                    foreach (Merlin32Directives token in Enum.GetValues(typeof(Merlin32Directives)))
                    {
                        if (token.ToString() == Merlin32Directives.ELUP.ToString())
                        {
                            strList.Add(Resources.directives.ELUPValue);
                        }
                        else
                        {
                            strList.Add(token.ToString());
                        }
                    }
                    foreach (Merlin32DataDefines token in Enum.GetValues(typeof(Merlin32DataDefines)))
                    {
                        strList.Add(token.ToString());
                    }
                }

                // OG We also need to replace "ELUP" with "--^"
                // OG 2015/10/21
                strList.Sort();
                // strList[strList.IndexOf(Merlin32Directives.ELUP.ToString())] = "--^";
                // OG
            }
            m_compList = new List<Completion>();
            foreach (string str in strList)
                m_compList.Add(new Completion(str, str, str, null, null));
            /*
            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            var triggerPoint = (SnapshotPoint)session.GetTriggerPoint(snapshot);

            if (triggerPoint == null)
                return;

            var line = triggerPoint.GetContainingLine();
            SnapshotPoint start = triggerPoint;

            while (start > line.Start && !char.IsWhiteSpace((start - 1).GetChar()))
            {
                start -= 1;
            }

            var applicableTo = snapshot.CreateTrackingSpan(new SnapshotSpan(start, triggerPoint), SpanTrackingMode.EdgeInclusive);
            */

            completionSets.Add(new CompletionSet("All", "All", FindTokenSpanAtPosition(session.GetTriggerPoint(m_buffer), session), m_compList, null));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            ITextStructureNavigator navigator = m_sourceprovider.NavigatorService.GetTextStructureNavigator(m_buffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }

        public void Dispose()
        {
            if (!m_isDisposed)
            {
                GC.SuppressFinalize(this);
                m_isDisposed = true;
            }
        }
    }
}

