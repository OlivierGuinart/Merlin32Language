using System;
using System.Collections.Generic;
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
        private Merlin32CompletionSourceProvider _sourceprovider;
        private ITextBuffer _buffer;
        private List<Completion> _compList;
        private bool _isDisposed;

        public Merlin32CompletionSource(Merlin32CompletionSourceProvider sourceprovider, ITextBuffer buffer)
        {
            _sourceprovider = sourceprovider;
            _buffer = buffer;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            if (_isDisposed)
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
                foreach (Merlin32Opcodes token in Enum.GetValues(typeof(Merlin32Opcodes)))
                {
                    strList.Add(char.IsLower(chTyped)? token.ToString().ToLower() : token.ToString());
                }
                foreach (Merlin32Directives token in Enum.GetValues(typeof(Merlin32Directives)))
                {
                    if ((token.ToString().ToLower() == Merlin32Directives.ELUP.ToString().ToLower()) || (token.ToString() == Merlin32Directives.ELUP.ToString()))
                    {
                        strList.Add(Resources.directives.ELUPValue);
                    }
                    else
                    {
                        strList.Add(char.IsLower(chTyped)? token.ToString().ToLower() : token.ToString());
                    }
                }
                foreach (Merlin32DataDefines token in Enum.GetValues(typeof(Merlin32DataDefines)))
                {
                    strList.Add(char.IsLower(chTyped)? token.ToString().ToLower() : token.ToString());
                }

                // OG We also need to replace "ELUP" with "--^"
                // OG 2015/10/21
                strList.Sort();
                // strList[strList.IndexOf(Merlin32Directives.ELUP.ToString())] = "--^";
                // OG
            }
            _compList = new List<Completion>();
            foreach (string str in strList)
                _compList.Add(new Completion(str, str, str, null, null));

            completionSets.Add(new CompletionSet("All", "All", FindTokenSpanAtPosition(session), _compList, null));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            ITextStructureNavigator navigator = _sourceprovider.NavigatorService.GetTextStructureNavigator(_buffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
    }
}

