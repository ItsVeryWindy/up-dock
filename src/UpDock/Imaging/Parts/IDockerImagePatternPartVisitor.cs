namespace UpDock.Imaging.Parts
{
    public interface IDockerImagePatternPartVisitor
    {
        void VisitEmptyDockerImagePatternPart(EmptyDockerImagePatternPart part);

        void VisitVersionDockerImagePatternPart(VersionDockerImagePatternPart part);

        void VisitDigestDockerImagePatternPart(DigestDockerImagePatternPart part);

        void VisitTextDockerImagePatternPart(TextDockerImagePatternPart part);
    }
}
