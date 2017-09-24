// Copyright (c) Microsoft Corporation
// All rights reserved
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using VSMerlin32.Coloring.Data;

namespace VSMerlin32.Coloring
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("Merlin32")]
    [TagType(typeof(Merlin32TokenTag))]
    internal sealed class Merlin32TokenTagProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new Merlin32TokenTagger(buffer) as ITagger<T>;
        }
    }

    public class Merlin32TokenTag : ITag
    {
        public Merlin32TokenTypes Tokentype { get; private set; }

        public Merlin32TokenTag(Merlin32TokenTypes type)
        {
            this.Tokentype = type;
        }
    }

    internal sealed class Merlin32TokenTagger : ITagger<Merlin32TokenTag>
    {
        private ITextBuffer _buffer;
        private readonly IDictionary<string, Merlin32TokenTypes> _merlin32Types;

        internal Merlin32TokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _merlin32Types = new Dictionary<string, Merlin32TokenTypes>();

            foreach (Merlin32TokenTypes token in Enum.GetValues(typeof(Merlin32TokenTypes)))
                _merlin32Types.Add(token.ToString(), token);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        // OG !!!
        public IEnumerable<ITagSpan<Merlin32TokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan curSpan in spans)
            {
                ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();
                int curLoc = containingLine.Start.Position;

                string formattedLine = containingLine.GetText();

                foreach (SnapshotHelper item in Merlin32CodeHelper.GetTokens(curSpan))
                {
                    if (item.Snapshot.IntersectsWith(curSpan))
                    {
                        yield return new TagSpan<Merlin32TokenTag>(item.Snapshot, new Merlin32TokenTag(item.TokenType));
                    }
                }

                //add an extra char location because of the space
                curLoc += formattedLine.Length + 1;
            }
        }
    }
}
