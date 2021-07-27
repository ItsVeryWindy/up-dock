using System;
using System.Collections.Immutable;
using NuGet.Versioning;

namespace UpDock.Nodes
{
    public interface ISearchTreeNode : IComparable<ISearchTreeNode>
    {
        SearchTreeNodeResult Search(ReadOnlySpan<char> span, int index, string? digest, ImmutableList<NuGetVersion> versions);
    }
}
