using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using VSMerlin32.Coloring;

namespace VSMerlin32
{
    
    [Export(typeof(IQuickInfoSourceProvider))]
    [ContentType("Merlin32")]
    [Name("Merlin32QuickInfo")]
    class Merlin32QuickInfoSourceProvider : IQuickInfoSourceProvider
    {

        [Import]
        IBufferTagAggregatorFactoryService aggService = null;

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new Merlin32QuickInfoSource(textBuffer, aggService.CreateTagAggregator<Merlin32TokenTag>(textBuffer));
        }
    }

    class Merlin32QuickInfoSource : IQuickInfoSource
    {
        private ITagAggregator<Merlin32TokenTag> _aggregator;
        private ITextBuffer _buffer;
        private Merlin32KeywordsHelper _Merlin32OpcodesHelper = new Merlin32KeywordsHelper();
        private bool _disposed = false;


        public Merlin32QuickInfoSource(ITextBuffer buffer, ITagAggregator<Merlin32TokenTag> aggregator)
        {
            _aggregator = aggregator;
            _buffer = buffer;
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;

            if (_disposed)
                throw new ObjectDisposedException("TestQuickInfoSource");

            var triggerPoint = (SnapshotPoint) session.GetTriggerPoint(_buffer.CurrentSnapshot);

            if (triggerPoint == null)
                return;

            foreach (IMappingTagSpan<Merlin32TokenTag> curTag in _aggregator.GetTags(new SnapshotSpan(triggerPoint, triggerPoint)))
            {
                if ((curTag.Tag.Tokentype == Merlin32TokenTypes.Merlin32Opcode) || (curTag.Tag.Tokentype == Merlin32TokenTypes.Merlin32Directive) || (curTag.Tag.Tokentype == Merlin32TokenTypes.Merlin32DataDefine))
                {
                    var tagSpan = curTag.Span.GetSpans(_buffer).First();
                    // Before
                    //if (tagSpan.GetText() == Merlin32Opcodes.ORG.ToString())
                    //{
                    //    applicableToSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(tagSpan, SpanTrackingMode.EdgeExclusive);
                    //    quickInfoContent.Add("Must be followed by the program's origin, e.g. org $800");
                    //}
                    // OG After
                    applicableToSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(tagSpan, SpanTrackingMode.EdgeExclusive);
                    if (tagSpan.GetText() == Resources.directives.ELUPValue)
                    {
                        quickInfoContent.Add(_Merlin32OpcodesHelper._Merlin32KeywordsQuickInfo[Merlin32Directives.ELUP.ToString()]);
                    }
                    else
                    {
                        // TODO: why do I get an exception here if I don't test for string.Empty!? 
                        /*
                         System.Collections.Generic.KeyNotFoundException was unhandled by user code
                          HResult=-2146232969
                          Message=The given key was not present in the dictionary.
                          Source=mscorlib
                          StackTrace:
                               at System.Collections.Generic.Dictionary`2.get_Item(TKey key)
                               at VSMerlin32.Merlin32QuickInfoSource.AugmentQuickInfoSession(IQuickInfoSession session, IList`1 quickInfoContent, ITrackingSpan& applicableToSpan) in c:\Users\Olivier\Documents\Visual Studio 2013\Projects\Merlin32 Language Service\Merlin32Language\Intellisense\Merlin32QuickInfoSource.cs:line 77
                               at Microsoft.VisualStudio.Language.Intellisense.Implementation.QuickInfoSession.Recalculate()
                               at Microsoft.VisualStudio.Language.Intellisense.Implementation.QuickInfoSession.Start()
                               at Microsoft.VisualStudio.Language.Intellisense.Implementation.DefaultQuickInfoController.OnTextView_MouseHover(Object sender, MouseHoverEventArgs e)
                               at Microsoft.VisualStudio.Text.Editor.Implementation.WpfTextView.RaiseHoverEvents()
                          InnerException: 
                         */
                        // Compare with changeset 151, you'll see why I ask...
                        if (string.Empty != tagSpan.GetText())
                        {
                            quickInfoContent.Add(_Merlin32OpcodesHelper._Merlin32KeywordsQuickInfo[tagSpan.GetText().ToUpper()]);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}

