using System;
using System.Collections.Generic;
using System.Linq;
using DockerUpgradeTool.Imaging.Parts;
using NuGet.Versioning;

namespace DockerUpgradeTool.Imaging
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

        private static readonly object Version = new object();

        public static DockerImageTemplatePattern Parse(string pattern, string group, DockerImageTemplate template)
        {
            IList<object> parts = new List<object>();

            var strStart = 0;

            var versionCount = 0;

            var span = pattern.AsSpan();

            for (var i = 0; i < span.Length;)
            {
                if(!IsVersion(span.Slice(i)))
                {
                    i++;
                    continue;
                }

                if(i != strStart)
                {
                    parts.Add(span.Slice(strStart, i - strStart).ToString());
                }

                parts.Add(Version);

                versionCount++;

                i += 3;
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
                if (part == Version)
                {
                    currentPart = new VersionDockerImagePatternPart(currentPart);
                }
                else
                {
                    currentPart = new TextDockerImagePatternPart(part.ToString()!, currentPart);
                }
            }

            return new DockerImageTemplatePattern(group, currentPart, template);
        }

        private static bool IsVersion(ReadOnlySpan<char> span)
        {
            return !span.IsEmpty && span.StartsWith("{v}");
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
