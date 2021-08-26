using System.Collections.Generic;
using NuGet.Versioning;

namespace UpDock.Imaging.Parts
{
    public interface IDockerImagePatternPart
    {
        IDockerImagePatternPart? Next { get; }

        void EnsureExecution(string? digest, IEnumerable<NuGetVersion> versions);

        string Execute(string? digest, IEnumerable<NuGetVersion> versions);

        string ToString();

        void Accept(IDockerImagePatternPartVisitor visitor);
    }
}
