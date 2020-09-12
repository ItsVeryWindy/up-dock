using DockerUpgradeTool.Imaging;

namespace DockerUpgradeTool.Nodes
{
    public readonly struct SearchTreeNodeResult
    {
        public DockerImagePattern? Pattern { get; }
        public int EndIndex { get; }

        public SearchTreeNodeResult(DockerImagePattern? pattern, int endIndex)
        {
            Pattern = pattern;
            EndIndex = endIndex;
        }

        internal void Deconstruct(out DockerImagePattern? image, out int endIndex)
        {
            image = Pattern;
            endIndex = EndIndex;
        }
    }
}
