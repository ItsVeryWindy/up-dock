using System;
using System.Collections.Generic;
using System.Linq;
using UpDock.Imaging.Parts;
using NuGet.Versioning;

namespace UpDock.Imaging
{
    public class DockerImageTemplatePattern
    {
        public IDockerImagePatternPart Part { get; }

        public DockerImageTemplate Template { get; }

        public string Group { get; }

        private DockerImageTemplatePattern(string group, IDockerImagePatternPart part, DockerImageTemplate template)
        {
            Group = @group;
            Part = part;
            Template = template;
        }

        public DockerImagePattern Create(IReadOnlyList<NuGetVersion> versions)
        {
            var image = Template.CreateImage(versions);

            return new DockerImagePattern(this, image);
        }

        public static DockerImageTemplatePattern Parse(string pattern, string group, DockerImageTemplate template)
        {
            IList<object> parts = new List<object>();

            var strStart = 0;

            var versionCount = 0;

            var span = pattern.AsSpan();

            for (var i = 0; i < span.Length;)
            {
                var (range, length) = ParseFloatRange(span.Slice(i));

                if (range == null)
                {
                    i++;
                    continue;
                }

                if(i != strStart)
                {
                    parts.Add(span[strStart..i].ToString());
                }

                parts.Add(range);

                versionCount++;

                i += length;
                strStart = i;
            }

            if (strStart != span.Length)
            {
                parts.Add(span.Slice(strStart).ToString());
            }

            if (versionCount != template.Versions.Count())
            {
                throw new FormatException("Pattern does not contain the same number of versions as the template");
            }

            IDockerImagePatternPart currentPart = EmptyDockerImagePatternPart.Instance;

            foreach (var part in parts.Reverse())
            {
                if (part is FloatRange range)
                {
                    currentPart = new VersionDockerImagePatternPart(range, currentPart);
                }
                else
                {
                    currentPart = new TextDockerImagePatternPart(part.ToString()!, currentPart);
                }
            }

            return new DockerImageTemplatePattern(group, currentPart, template);
        }

        private const string VersionStart = "{v";
        private const char VersionEnd = '}';
        private static readonly FloatRange AnyVersion = FloatRange.Parse("*");

        private static (FloatRange?, int length) ParseFloatRange(ReadOnlySpan<char> span)
        {
            if (span.IsEmpty)
                return (null, 0);

            if (!span.StartsWith(VersionStart))
                return (null, 0);

            var remainingVersion = span.Slice(2);

            var closeBracketIndex = remainingVersion.IndexOf(VersionEnd);

            if (closeBracketIndex < 0)
                throw new FormatException("The image tag for a template should have matching curly brackets.");

            if (closeBracketIndex == 0)
                return (AnyVersion, VersionStart.Length + 1);

            var length = closeBracketIndex + VersionStart.Length + 1;

            var range = FloatRange.Parse(remainingVersion.Slice(0, closeBracketIndex).ToString());

            if (range == null)
                throw new FormatException("The image tag for the template contains an invalid version range.");

            return (range, length);
        }

        public override string ToString() => Part.ToString();

        public override int GetHashCode() => HashCode.Combine(Part, Template);

        public override bool Equals(object? obj)
        {
            if (!(obj is DockerImageTemplatePattern pattern))
                return false;

            return Equals(Part, pattern.Part) && Equals(Template, pattern.Template);
        }
    }
}
