using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DockerUpgradeTool.Imaging;
using DockerUpgradeTool.Nodes;
using NuGet.Versioning;

namespace DockerUpgradeTool.Tests
{
    public class StubSearchNode : ISearchTreeNode
    {
        public static readonly StubSearchNode Instance = new StubSearchNode();

        public static readonly IEnumerable<ISearchTreeNode> SingleInstance = Enumerable.Repeat<ISearchTreeNode>(Instance, 1);

        public static readonly DockerImagePattern Image = DockerImageTemplate.ParseTemplate("test:{v}").CreatePattern(true, true).Create(new List<NuGetVersion>
        {
            new NuGetVersion(1, 2, 3)
        });

        public int CompareTo(ISearchTreeNode? other) => 1;

        public SearchTreeNodeResult Search(ReadOnlySpan<char> span, int endIndex, ImmutableList<NuGetVersion> versions) => new SearchTreeNodeResult(Image, endIndex);
    }
}
