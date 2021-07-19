using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NuGet.Versioning;

namespace UpDock.Nodes
{
    public class FloatRangeNode : ISearchTreeNode
    {
        private readonly FloatRange _range;
        private readonly IEnumerable<ISearchTreeNode> _children;

        public FloatRangeNode(FloatRange range, IEnumerable<ISearchTreeNode> children)
        {
            _range = range;
            _children = children;
        }

        public SearchTreeNodeResult Search(ReadOnlySpan<char> span, int index, ImmutableList<NuGetVersion> versions)
        {
            var version = versions.Last();

            if (!_range.Satisfies(version))
                return new SearchTreeNodeResult();

            return GetChildResult(span, index, versions);
        }

        private SearchTreeNodeResult GetChildResult(ReadOnlySpan<char> span, int index, ImmutableList<NuGetVersion> versions)
        {
            foreach (var child in _children)
            {
                var childResult = child.Search(span, index, versions);

                if (childResult.Pattern != null)
                {
                    return childResult;
                }
            }

            return new SearchTreeNodeResult();
        }

        public int CompareTo(ISearchTreeNode? other) => -1;
    }
}
