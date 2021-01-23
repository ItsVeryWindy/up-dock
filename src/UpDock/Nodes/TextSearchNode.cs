using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NuGet.Versioning;

namespace UpDock.Nodes
{
    public class TextSearchNode : ISearchTreeNode
    {
        private readonly string _text;
        private readonly List<ISearchTreeNode> _children;

        public TextSearchNode(string text, IEnumerable<ISearchTreeNode> children)
        {
            _text = text;
            _children = new List<ISearchTreeNode>(children);
        }

        public SearchTreeNodeResult Search(ReadOnlySpan<char> span, int endIndex, ImmutableList<NuGetVersion> versions)
        {
            var result = span.StartsWith(_text, StringComparison.InvariantCultureIgnoreCase);

            return result ? GetChildResult(span.Slice(_text.Length), endIndex + _text.Length, versions) : new SearchTreeNodeResult();
        }

        private SearchTreeNodeResult GetChildResult(ReadOnlySpan<char> span, int endIndex, ImmutableList<NuGetVersion> versions)
        {
            foreach (var child in _children)
            {
                var childResult = child.Search(span, endIndex, versions);

                if (childResult.Pattern != null)
                {
                    return childResult;
                }
            }

            return new SearchTreeNodeResult();
        }

        public int CompareTo(ISearchTreeNode? other)
        {
            if (other is TextSearchNode textSearchNode)
            {
                return _text.Length >= textSearchNode._text.Length ? -1 : 1;
            }

            if (other is VersionSearchNode)
                return 1;

            return -1;
        }
    }
}
