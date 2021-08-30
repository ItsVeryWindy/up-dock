using System;
using System.Collections.Generic;
using System.Linq;
using UpDock.Imaging.Parts;
using NuGet.Versioning;

namespace UpDock
{
    public class VersionDockerImagePatternPart : IDockerImagePatternPart
    {
        public IDockerImagePatternPart Next { get; }
        public FloatRange Range { get; }

        public VersionDockerImagePatternPart(FloatRange range, IDockerImagePatternPart next)
        {
            Range = range;
            Next = next;
        }

        public void EnsureExecution(string? digest, IEnumerable<NuGetVersion> versions)
        {
            if(!versions.Any())
                throw new ArgumentException("Pattern does not have the same number versions as the image", nameof(versions));

            Next.EnsureExecution(digest, versions.Skip(1));
        }

        public string Execute(string? digest, IEnumerable<NuGetVersion> versions)
        {
            var version = versions.First();

            return version + Next.Execute(digest, versions.Skip(1));
        }

        public override string ToString() => $"{{v{Range}}}{Next}";

        public override int GetHashCode() => HashCode.Combine(Range, Next);

        public override bool Equals(object? obj)
        {
            if (obj is not VersionDockerImagePatternPart part)
                return false;

            return Equals(Next, part.Next);
        }

        public void Accept(IDockerImagePatternPartVisitor visitor) => visitor.Visit(this);
    }
}
