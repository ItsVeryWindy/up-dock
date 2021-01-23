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

        public VersionDockerImagePatternPart(IDockerImagePatternPart next)
        {
            Next = next;
        }

        public void EnsureExecution(IEnumerable<NuGetVersion> versions)
        {
            if(!versions.Any())
                throw new ArgumentException("Pattern does not have the same number versions as the image", nameof(versions));

            Next.EnsureExecution(versions.Skip(1));
        }

        public string Execute(IEnumerable<NuGetVersion> versions)
        {
            var version = versions.First();

            return version + Next.Execute(versions.Skip(1));
        }

        public override string ToString() => "{v}" + Next.ToString();

        private static readonly object Version = new object();

        public override int GetHashCode() => HashCode.Combine(Version, Next);

        public override bool Equals(object? obj)
        {
            if (!(obj is VersionDockerImagePatternPart part))
                return false;

            return Equals(Next, part.Next);
        }
    }
}
