using System;
using System.Collections.Immutable;
using NuGet.Versioning;

namespace DockerUpgrader.Nodes
{
    public interface ISearchTreeNode : IComparable<ISearchTreeNode>
    {
        SearchTreeNodeResult Search(ReadOnlySpan<char> span, int index, ImmutableList<NuGetVersion> versions);
    }
}