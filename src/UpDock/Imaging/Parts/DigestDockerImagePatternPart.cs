using System;
using System.Collections.Generic;
using NuGet.Versioning;

namespace UpDock.Imaging.Parts
{
    public class DigestDockerImagePatternPart : IDockerImagePatternPart
    {
        public IDockerImagePatternPart Next { get; }

        public DigestDockerImagePatternPart(IDockerImagePatternPart next)
        {
            Next = next;
        }

        public void EnsureExecution(string? digest, IEnumerable<NuGetVersion> versions)
        {
        }

        public string Execute(string? digest, IEnumerable<NuGetVersion> versions) => digest + Next.Execute(digest, versions);

        public override string ToString() => $"{{digest}}{Next}";

        private static readonly object Digest = new();

        public override int GetHashCode() => HashCode.Combine(Digest, Next);

        public override bool Equals(object? obj)
        {
            if (obj is not DigestDockerImagePatternPart part)
                return false;

            return Equals(Next, part.Next);
        }

        public void Accept(IDockerImagePatternPartVisitor visitor) => visitor.Visit(this);
    }
}
