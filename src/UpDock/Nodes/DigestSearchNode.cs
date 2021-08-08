using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NuGet.Versioning;

namespace UpDock.Nodes
{
    public class DigestSearchNode : ISearchTreeNode
    {
        private const string DigestStart = "sha256:";
        private const int DigestLength = 64;

        private readonly List<ISearchTreeNode> _children;

        public DigestSearchNode(IEnumerable<ISearchTreeNode> children)
        {
            _children = new List<ISearchTreeNode>(children);
        }

        public SearchTreeNodeResult Search(ReadOnlySpan<char> span, int endIndex, string? digest, ImmutableList<NuGetVersion> versions)
        {
            var length = DigestStart.Length + DigestLength;

            var sha = span.Slice(0, length);

            if (!sha.StartsWith(DigestStart))
                return new();

            if (sha.Length < length)
                return new();

            foreach(var chr in sha[DigestStart.Length..])
            {
                if ((chr < 'a' || chr > 'z') && !char.IsDigit(chr))
                    return new();
            }

            return GetChildResult(span[length..], endIndex + length, sha.ToString(), versions);
        }

        private SearchTreeNodeResult GetChildResult(ReadOnlySpan<char> span, int endIndex, string? digest, ImmutableList<NuGetVersion> versions)
        {
            foreach (var child in _children)
            {
                var childResult = child.Search(span, endIndex, digest, versions);

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
