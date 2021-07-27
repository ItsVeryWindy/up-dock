using System;
using System.Collections.Generic;
using NuGet.Versioning;

namespace UpDock.Imaging.Parts
{
    public class TextDockerImagePatternPart : IDockerImagePatternPart
    {
        public string Text { get; }
        public IDockerImagePatternPart Next { get; }

        public TextDockerImagePatternPart(string text, IDockerImagePatternPart next)
        {
            Text = text;
            Next = next;
        }

        public void EnsureExecution(string? digest, IEnumerable<NuGetVersion> versions)
        {
        }

        public string Execute(string? digest, IEnumerable<NuGetVersion> versions) => Text + Next.Execute(digest, versions);

        public override string ToString() => Text + Next.ToString();

        public override int GetHashCode() => HashCode.Combine(Text, Next);

        public override bool Equals(object? obj)
        {
            if (!(obj is TextDockerImagePatternPart part))
                return false;

            return Equals(Text, part.Text) && Equals(Next, part.Next);
        }
    }
}
