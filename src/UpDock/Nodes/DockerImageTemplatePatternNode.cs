using UpDock.Imaging;

namespace UpDock.Nodes
{
    public class DockerImageTemplatePatternNode : ISearchTreeNode
    {
        private readonly DockerImageTemplatePattern _pattern;

        public DockerImageTemplatePatternNode(DockerImageTemplatePattern pattern)
        {
            _pattern = pattern;
        }

        public SearchTreeNodeResult Search(SearchTreeNodeContext context) => new(_pattern.Create(context.Digest.IsEmpty ? null : context.Digest.ToString(), context.Versions), context.Index);

        public int CompareTo(ISearchTreeNode? other) => 1;
    }
}
