using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DockerUpgradeTool.Nodes;
using NuGet.Versioning;

namespace DockerUpgradeTool.Imaging
{
    public class DockerImageTemplate
    {
        public Uri Repository { get; }

        private readonly string _image;

        public string Image => ImageString();

        public string Tag => TagString();

        public IEnumerable<FloatRange> Versions => _parts.OfType<FloatRange>();

        private readonly List<object> _parts;

        private DockerImageTemplate(Uri repository, string image, List<object> parts)
        {
            Repository = repository;
            _image = image;
            _parts = parts;
        }

        public bool Satisfies(DockerImage image)
        {
            var ranges = _parts.OfType<FloatRange>().ToList();

            var versions = image.Versions.ToList();

            if (ranges.Count != versions.Count)
                return false;

            for (var i = 0; i < versions.Count; i++)
            {
                var range = ranges[i];
                var version = versions[i];

                if (!range.Satisfies(version))
                    return false;
            }

            return true;
        }

        public DockerImage CreateImage(IReadOnlyList<NuGetVersion> versions)
        {
            if(versions.Count != _parts.OfType<FloatRange>().Count())
                throw new ArgumentException("Versions given do not match template", nameof(versions));

            var parts = new List<object>();

            var versionCounter = 0;

            foreach (var part in _parts)
            {
                if (part is FloatRange)
                {
                    var version = versions[versionCounter++];

                    parts.Add(version);

                    continue;
                }

                parts.Add(part);
            }

            return new DockerImage(Repository, Image, parts, this);
        }

        public static readonly Uri DefaultRepository = new Uri("https://registry-1.docker.io");

        public static DockerImageTemplate Parse(string str)
        {
            var (repository, image, tag) = SplitString(str);

            return CreateTemplate(repository ?? DefaultRepository, image, tag);
        }

        public static DockerImageTemplate Parse(string str, Uri repository)
        {
            var (_, image, tag) = SplitString(str);

            return CreateTemplate(repository, image, tag);
        }

        private static DockerImageTemplate CreateTemplate(Uri repository, string image, string tag)
        {
            var parts = ParseTag(tag);

            return new DockerImageTemplate(repository, image, parts);
        }

        private static List<object> ParseTag(string tag)
        {
            var parts = new List<object>();

            var strStart = 0;

            if (tag.StartsWith('.') || tag.StartsWith('-'))
                throw new FormatException("The image tag cannot start with a period or a dash.");

            for (var i = 0; i < tag.Length;)
            {
                var (range, length) = ParseFloatRange(tag.AsSpan(i));

                if (range == null)
                {
                    i++;
                    continue;
                }

                if(i != strStart)
                {
                    parts.Add(ParseTagPart(tag[strStart..i]));
                }

                parts.Add(range);
                i += length;
                strStart = i;
            }

            if (strStart != tag.Length)
            {
                parts.Add(ParseTagPart(tag.Substring(strStart)));
            }

            return parts;
        }

        private static string ParseTagPart(string tagPart)
        {
            if (tagPart.All(x => x >= 'a' && x <= 'z' || x >= 'A' && x <= 'Z' || x >= '0' && x <= '9' || x == '.' || x == '_' || x == '-'))
                return tagPart;

            throw new FormatException("The image tag should only contain lowercase and uppercase letters, digits, periods, underscores, or dashes.");
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

            if(closeBracketIndex < 0)
                throw new FormatException("The image tag is missing closing bracket.");

            if (closeBracketIndex == 0)
                return (AnyVersion, VersionStart.Length + 1);

            var length = closeBracketIndex + VersionStart.Length + 1;

            var range = FloatRange.Parse(remainingVersion.Slice(0, closeBracketIndex).ToString());

            if (range == null)
                throw new FormatException("The image tag contains an invalid version range.");

            return (range, length);
        }

        private static (Uri? repository, string image, string tag) SplitString(string str)
        {
            var (repository, image) = ParseRepository(str);

            string tag;

            (image, tag) = ParseImage(image);

            return (repository, image, tag);
        }
        private static (Uri? repository, string image) ParseRepository(string str)
        {
            var imageSplit = str.Split('/', 2);

            if (imageSplit.Length == 1 || !imageSplit[0].Contains("."))
            {
                return (null, str);
            }

            if (!Uri.TryCreate($"https://{imageSplit[0]}", UriKind.Absolute, out var repository))
                    throw new FormatException("The registry name is invalid");
            
            if (repository.Host.Contains('_') == true)
                throw new FormatException("The registry name should not contain underscores.");

            return (repository, imageSplit[1]);
        }

        private static (string image, string tag) ParseImage(string str)
        {
            var split = str.Split(":");

            var image = split[0];

            if (image.StartsWith('.') || image.StartsWith('_') || image.StartsWith('-'))
                throw new FormatException("The image name should not begin with a period, an underscore or a dash.");

            if (image.EndsWith('.') || image.EndsWith('_') || image.EndsWith('-'))
                throw new FormatException("The image name should not end with a period, an underscore or a dash.");

            if (!image.All(x => x >= 'a' && x <= 'z' || x >= '0' && x <= '9' || x == '.' || x == '_' || x == '-' || x == '/'))
                throw new FormatException("The image name should only contain lowercase letters, digits, periods, underscores, or dashes.");

            if (split.Length == 1)
                return (image, "{v}");

            if(split.Length != 2)
                throw new FormatException("The image name should only have one colon.");

            return (image, split[1]);
        }

        private const string DefaultLibraryName = "library/";

        private string ImageString()
        {
            var sb = new StringBuilder();

            if (Repository == DefaultRepository && !_image.StartsWith("library/"))
            {
                sb.Append(DefaultLibraryName);
            }

            sb.Append(_image);

            return sb.ToString();
        }

        private string TagString()
        {
            var sb = new StringBuilder();

            foreach (var part in _parts)
            {
                sb.Append(part is FloatRange range ? $"{{v{range}}}" : part);
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if(Repository != DefaultRepository)
            {
                sb.Append(Repository.Host).Append("/");
            }

            sb.Append(Image).Append(":").Append(Tag);

            return sb.ToString();
        }

        public string ToRepositoryImageString()
        {
            var sb = new StringBuilder();

            if(Repository != DefaultRepository)
            {
                sb.Append(Repository.Host).Append("/");
            }

            sb.Append(Image);

            return sb.ToString();
        }

        public DockerImageTemplatePattern CreatePattern(bool includeRepository, bool includeImage) =>
            CreatePattern(includeRepository, includeImage, null);

        public DockerImageTemplatePattern CreatePattern(bool includeRepository, bool includeImage, string? group)
        {
            var sb = new StringBuilder();

            if(Repository != DefaultRepository && includeRepository)
            {
                sb.Append(Repository.Host).Append("/");
            }

            if (includeImage)
            {
                sb.Append(_image).Append(":");
            }

            foreach (var part in _parts)
            {
                sb.Append(part is FloatRange ? "{v}" : part);
            }

            var pattern = sb.ToString();

            return DockerImageTemplatePattern.Parse(pattern, group ?? pattern, this);
        }

        public DockerImageTemplatePattern CreatePattern(string pattern) => CreatePattern(pattern, null);

        public DockerImageTemplatePattern CreatePattern(string pattern, string? group) => DockerImageTemplatePattern.Parse(pattern, group ?? pattern, this);

        public override int GetHashCode() => HashCode.Combine(Repository, Image, Tag);

        public override bool Equals(object? obj)
        {
            if (!(obj is DockerImageTemplate template))
                return false;

            return Equals(Repository, template.Repository) && Equals(Image, template.Image) && Equals(Tag, template.Tag);
        }
    }
}
