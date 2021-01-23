using System;
using System.Collections.Immutable;
using UpDock.Imaging;
using NuGet.Versioning;

namespace UpDock.Nodes
{
    public class DockerImageTemplatePatternNode : ISearchTreeNode
    {
        private readonly DockerImageTemplatePattern _pattern;

        public DockerImageTemplatePatternNode(DockerImageTemplatePattern pattern)
        {
            _pattern = pattern;
        }

        public SearchTreeNodeResult Search(ReadOnlySpan<char> span, int endIndex, ImmutableList<NuGetVersion> versions) => new SearchTreeNodeResult(_pattern.Create(versions), endIndex);

        public int CompareTo(ISearchTreeNode? other) => 1;
    }
}
