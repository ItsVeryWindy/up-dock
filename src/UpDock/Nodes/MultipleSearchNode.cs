using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NuGet.Versioning;

namespace UpDock.Nodes
{
    public class MultipleSearchNode : ISearchTreeNode
    {
        private readonly List<ISearchTreeNode> _children;

        public MultipleSearchNode(IEnumerable<ISearchTreeNode> children)
        {
            _children = new List<ISearchTreeNode>(children);
        }

        public SearchTreeNodeResult Search(ReadOnlySpan<char> span, int endIndex, string? digest, ImmutableList<NuGetVersion> versions)
        {
            foreach (var child in _children)
            {
                var childResult = child.Search(span, endIndex, digest, versions);

                if (childResult.Pattern != null)
                    return childResult;
            }

            return new SearchTreeNodeResult();
        }

        public int CompareTo(ISearchTreeNode? other) => -1;
    }
}
