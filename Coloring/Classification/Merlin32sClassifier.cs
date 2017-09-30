using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.StandardClassification;
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
        internal IBufferTagAggregatorFactoryService AggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITagAggregator<Merlin32TokenTag> merlin32TagAggregator =
                                            AggregatorFactory.CreateTagAggregator<Merlin32TokenTag>(buffer);

            return new Merlin32Classifier(buffer, merlin32TagAggregator, ClassificationTypeRegistry) as ITagger<T>;
        }
    }

    internal sealed class Merlin32Classifier : ITagger<ClassificationTag>
    {
        private ITextBuffer _buffer;
        private readonly ITagAggregator<Merlin32TokenTag> _aggregator;
        private readonly IDictionary<Merlin32TokenTypes, IClassificationType> _merlin32Types;

        internal Merlin32Classifier(ITextBuffer buffer,
                               ITagAggregator<Merlin32TokenTag> merlin32TagAggregator,
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _aggregator = merlin32TagAggregator;
            _merlin32Types = new Dictionary<Merlin32TokenTypes, IClassificationType>();

			_merlin32Types[Merlin32TokenTypes.Merlin32Comment] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment);
			_merlin32Types[Merlin32TokenTypes.Merlin32Text] = typeService.GetClassificationType(PredefinedClassificationTypeNames.String);
			_merlin32Types[Merlin32TokenTypes.Merlin32Opcode] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Keyword);
			_merlin32Types[Merlin32TokenTypes.Merlin32DataDefine] = typeService.GetClassificationType(PredefinedClassificationTypeNames.SymbolDefinition);
			_merlin32Types[Merlin32TokenTypes.Merlin32Directive] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Operator);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var tagSpan in _aggregator.GetTags(spans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                yield return
                    new TagSpan<ClassificationTag>(tagSpans[0], new ClassificationTag(_merlin32Types[tagSpan.Tag.Tokentype]));
            }
        }
    }
}
