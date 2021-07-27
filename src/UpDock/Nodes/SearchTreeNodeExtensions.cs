using System;
using System.Collections.Immutable;
using NuGet.Versioning;

namespace UpDock.Nodes
{
    public static class SearchTreeNodeExtensions
    {
        public static SearchTreeNodeResult Search(this ISearchTreeNode node, ReadOnlySpan<char> span) => node.Search(span, 0, null, ImmutableList<NuGetVersion>.Empty);
    }
}
