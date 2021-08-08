using System.Collections.Generic;
using NuGet.Versioning;

namespace UpDock.Imaging.Parts
{
    public class EmptyDockerImagePatternPart : IDockerImagePatternPart
    {
        public static readonly EmptyDockerImagePatternPart Instance = new();

        public IDockerImagePatternPart? Next { get; } = null;

        private EmptyDockerImagePatternPart()
        {

        }

        public void EnsureExecution(string? digest, IEnumerable<NuGetVersion> versions)
        {
        }

        public string Execute(string? digest, IEnumerable<NuGetVersion> versions) => ToString();

        public override string ToString() => string.Empty;
    }
}
