namespace UpDock.Imaging.Parts
{
    public interface IDockerImagePatternPartVisitor
    {
        void Visit(EmptyDockerImagePatternPart part);

        void Visit(VersionDockerImagePatternPart part);

        void Visit(DigestDockerImagePatternPart part);

        void Visit(TextDockerImagePatternPart part);
    }
}
