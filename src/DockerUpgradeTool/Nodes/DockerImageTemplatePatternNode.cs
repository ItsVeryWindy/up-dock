using System;
using System.Collections.Immutable;
using DockerUpgradeTool.Imaging;
using NuGet.Versioning;

namespace DockerUpgradeTool.Nodes
{
    public class DockerImageTemplatePatternNode : ISearchTreeNode
    {
        private readonly DockerImageTemplatePattern _pattern;

        public DockerImageTemplatePatternNode(DockerImageTemplatePattern pattern)
        {
            _pattern = pattern;
        }

        public SearchTreeNodeResult Search(ReadOnlySpan<char> span, int endIndex, ImmutableList<NuGetVersion> versions)
        {
            return new SearchTreeNodeResult(_pattern.Create(versions), endIndex);
        }

        public int CompareTo(ISearchTreeNode? other)
        {
            return 1;
        }
    }
}
