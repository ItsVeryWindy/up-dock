using DockerUpgrader.Imaging.Parts;
using DockerUpgrader.Nodes;

namespace DockerUpgrader.Imaging
{
    public class DockerImagePattern
    {
        public DockerImage Image { get; }

        public DockerImageTemplatePattern Pattern { get; }

        public DockerImagePattern(DockerImageTemplatePattern pattern, DockerImage image)
        {
            Pattern = pattern;
            Image = image;

            Pattern.Part.EnsureExecution(image.Versions);
        }

        public override string ToString() => Pattern.Part.Execute(Image.Versions);

        public DockerImagePattern Create(DockerImage image)
        {
            return new DockerImagePattern(Pattern, image);
        }
    }
}
