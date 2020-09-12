using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NuGet.Versioning;

namespace DockerUpgradeTool.Nodes
{
    public class VersionSearchNode : ISearchTreeNode
    {
        private static readonly HashSet<char> ValidCharacters;

        private readonly List<ISearchTreeNode> _children;

        static VersionSearchNode()
        {
            ValidCharacters = new HashSet<char>();

            for(var i = 'a'; i <= 'z'; i++)
            {
                ValidCharacters.Add(i);
            }

            for(var i = 'A'; i <= 'Z'; i++)
            {
                ValidCharacters.Add(i);
            }

            for(var i = '0'; i <= '9'; i++)
            {
                ValidCharacters.Add(i);
            }

            ValidCharacters.Add('.');
            ValidCharacters.Add('-');
        }

        public VersionSearchNode(IEnumerable<ISearchTreeNode> children)
        {
            _children = new List<ISearchTreeNode>(children);
        }

        public SearchTreeNodeResult Search(ReadOnlySpan<char> span, int endIndex, ImmutableList<NuGetVersion> versions)
        {
            var versionEndIndex = GetEndOfVersion(span);

            for (var i = 0; i < versionEndIndex; i++)
            {
                var subString = span.Slice(0, versionEndIndex - i).ToString();

                if (!NuGetVersion.TryParse(subString, out var version))
                    continue;

                var childResult = GetChildResult(span.Slice(subString.Length), endIndex + subString.Length, versions.Add(version));

                if(childResult.Pattern == null)
                    continue;

                return childResult;
            }

            return new SearchTreeNodeResult();
        }

        private static int GetEndOfVersion(ReadOnlySpan<char> span)
        {
            for (var i = 0; i < span.Length; i++)
            {
                var chr = span[i];

                if (!ValidCharacters.Contains(chr))
                {
                    return i;
                }
            }

            return span.Length;
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
            return -1;
        }
    }
}
