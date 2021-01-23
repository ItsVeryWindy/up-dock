using System.Collections.Generic;
using NuGet.Versioning;

namespace UpDock.Imaging.Parts
{
    public class EmptyDockerImagePatternPart : IDockerImagePatternPart
    {
        public static readonly EmptyDockerImagePatternPart Instance = new EmptyDockerImagePatternPart();

        public IDockerImagePatternPart? Next { get; } = null;

        private EmptyDockerImagePatternPart()
        {

        }

        public void EnsureExecution(IEnumerable<NuGetVersion> versions)
        {
        }

        public string Execute(IEnumerable<NuGetVersion> versions) => ToString();

        public override string ToString() => string.Empty;
    }
}
