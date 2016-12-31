using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace VSMerlin32.Coloring.Classification
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("Merlin32")]
    [TagType(typeof(ClassificationTag))]
    internal sealed class Merlin32ClassifierProvider : ITaggerProvider
    {

        [Export]
        [Name("Merlin32")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition Merlin32ContentType = null;

        [Export]
        [FileExtension(".s")]
        [ContentType("Merlin32")]
        internal static FileExtensionToContentTypeDefinition Merlin32FileType = null;

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {

            ITagAggregator<Merlin32TokenTag> Merlin32TagAggregator =
                                            aggregatorFactory.CreateTagAggregator<Merlin32TokenTag>(buffer);

            return new Merlin32Classifier(buffer, Merlin32TagAggregator, ClassificationTypeRegistry) as ITagger<T>;
        }
    }

    internal sealed class Merlin32Classifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        ITagAggregator<Merlin32TokenTag> _aggregator;
        IDictionary<Merlin32TokenTypes, IClassificationType> _Merlin32Types;

        internal Merlin32Classifier(ITextBuffer buffer,
                               ITagAggregator<Merlin32TokenTag> Merlin32TagAggregator,
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _aggregator = Merlin32TagAggregator;
            _Merlin32Types = new Dictionary<Merlin32TokenTypes, IClassificationType>();

            foreach (Merlin32TokenTypes token in Enum.GetValues(typeof(Merlin32TokenTypes)))
                _Merlin32Types[token] = typeService.GetClassificationType(token.ToString());
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {

            foreach (var tagSpan in this._aggregator.GetTags(spans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                yield return 
                    new TagSpan<ClassificationTag>(tagSpans[0], 
                        new ClassificationTag(_Merlin32Types[tagSpan.Tag.Tokentype]));
            }
        }
    }
}
