using System;
using System.Collections.Immutable;
using NuGet.Versioning;

namespace DockerUpgrader.Nodes
{
    public static class SearchTreeNodeExtensions
    {
        public static SearchTreeNodeResult Search(this ISearchTreeNode node, ReadOnlySpan<char> span)
        {
            return node.Search(span, 0, ImmutableList<NuGetVersion>.Empty);
        }
    }
}