using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DockerUpgrader.Imaging;
using DockerUpgrader.Nodes;
using NuGet.Versioning;

namespace DockerUpgrader.Tests
{
    public class StubSearchNode : ISearchTreeNode
    {
        public static readonly StubSearchNode Instance = new StubSearchNode();

        public static readonly IEnumerable<ISearchTreeNode> SingleInstance = Enumerable.Repeat<ISearchTreeNode>(Instance, 1);

        public static readonly DockerImagePattern Image = DockerImageTemplate.ParseTemplate("test:{v}").CreatePattern(true, true).Create(new List<NuGetVersion>
        {
            new NuGetVersion(1, 2, 3)
        });

        public int CompareTo(ISearchTreeNode? other)
        {
            return 1;
        }

        public SearchTreeNodeResult Search(ReadOnlySpan<char> span, int endIndex, ImmutableList<NuGetVersion> versions)
        {
            return new SearchTreeNodeResult(Image, endIndex);
        }
    }
}