using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UpDock.Nodes;
using NuGet.Versioning;

namespace UpDock.Imaging
{
    public class DockerImageTemplate
    {
        public Uri Repository { get; }

        private readonly string _image;

        public string Image => ImageString();

        public string Tag => TagString();

        public bool HasDigest { get; }

        public IEnumerable<FloatRange> Versions => _parts.OfType<FloatRange>();

        private readonly List<object> _parts;

        private DockerImageTemplate(Uri repository, string image, bool hasDigest, List<object> parts)
        {
            Repository = repository;
            _image = image;
            HasDigest = hasDigest;
            _parts = parts;
        }

        public DockerImage CreateImage(string? digest, IReadOnlyList<NuGetVersion> versions)
        {
            var requiresVersions = !HasDigest || (HasDigest && digest is null) || versions.Count > 0;

            if (requiresVersions && versions.Count != _parts.OfType<FloatRange>().Count())
                throw new ArgumentException("Versions given do not match template", nameof(versions));

            var parts = new List<object>();

            var versionCounter = 0;

            foreach (var part in _parts)
            {
                if (part is FloatRange)
                {
                    if (requiresVersions)
                    {
                        var version = versions[versionCounter++];

                        parts.Add(version);
                    }

                    continue;
                }

                parts.Add(part);
            }

            return new DockerImage(Repository, Image, digest, parts, this);
        }

        public static readonly Uri DefaultRepository = new("https://registry-1.docker.io");

        public static DockerImageTemplate Parse(string str)
        {
            var (repository, image, hasDigest, tag) = SplitString(str);

            return CreateTemplate(repository ?? DefaultRepository, image, hasDigest, tag);
        }

        private static DockerImageTemplate CreateTemplate(Uri repository, string image, bool hasDigest, string tag)
        {
            var parts = ParseTag(tag);

            return new DockerImageTemplate(repository, image, hasDigest, parts);
        }

        private static List<object> ParseTag(string tag)
        {
            var parts = new List<object>();

            var strStart = 0;

            if (tag.StartsWith('.') || tag.StartsWith('-'))
                throw new FormatException("The image tag for a template cannot start with a period or a dash.");

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
                parts.Add(ParseTagPart(tag[strStart..]));
            }

            return parts;
        }

        private static string ParseTagPart(string tagPart)
        {
            if (tagPart.All(x => x >= 'a' && x <= 'z' || x >= 'A' && x <= 'Z' || x >= '0' && x <= '9' || x == '.' || x == '_' || x == '-'))
                return tagPart;

            throw new FormatException("The image tag for a template should only contain lowercase and uppercase letters, digits, periods, underscores, or dashes.");
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

            var remainingVersion = span[2..];

            var closeBracketIndex = remainingVersion.IndexOf(VersionEnd);

            if(closeBracketIndex < 0)
                throw new FormatException("The image tag for a template should have matching curly brackets.");

            if (closeBracketIndex == 0)
                return (AnyVersion, VersionStart.Length + 1);

            var length = closeBracketIndex + VersionStart.Length + 1;

            var range = FloatRange.Parse(remainingVersion.Slice(0, closeBracketIndex).ToString());

            if (range == null)
                throw new FormatException("The image tag for the template contains an invalid version range.");

            return (range, length);
        }

        private static (Uri? repository, string image, bool hasDigest, string tag) SplitString(string str)
        {
            var (repository, image) = ParseRepository(str);

            bool hasDigest;
            string tag;

            (image, hasDigest, tag) = ParseImage(image);

            return (repository, image, hasDigest, tag);
        }
        private static (Uri? repository, string image) ParseRepository(string str)
        {
            var imageSplit = str.Split('/', 2);

            if (imageSplit.Length == 1 || !imageSplit[0].Contains('.'))
            {
                return (null, str);
            }

            if (!Uri.TryCreate($"https://{imageSplit[0]}", UriKind.Absolute, out var repository))
                    throw new FormatException("The registry name for the template is invalid.");
            
            if (repository.Host.Contains('_') == true)
                throw new FormatException("The registry name for a template should not contain underscores.");

            return (repository, imageSplit[1]);
        }

        private static (string image, bool hasDigest, string tag) ParseImage(string str)
        {
            var splitDigest = str.Split('@');

            var hasDigest = splitDigest.Length > 1;

            if (splitDigest.Length > 2)
                throw new FormatException("The image name for a template should only have one @ symbol.");

            var splitTag = splitDigest[hasDigest ? 1 : 0].Split(':');

            var image = hasDigest ? splitDigest[0] : splitTag[0];

            if (hasDigest && splitTag[0] != "{digest}")
                throw new FormatException("If an @ symbol is specified in the template, it must be followed by '{digest}' to indicate that a digest is required.");

            if (image.StartsWith('.') || image.StartsWith('_') || image.StartsWith('-'))
                throw new FormatException("The image name for a template should not begin with a period, an underscore or a dash.");

            if (image.EndsWith('.') || image.EndsWith('_') || image.EndsWith('-'))
                throw new FormatException("The image name for a template should not end with a period, an underscore or a dash.");

            if (!image.All(x => x >= 'a' && x <= 'z' || x >= '0' && x <= '9' || x == '.' || x == '_' || x == '-' || x == '/'))
                throw new FormatException("The image name for a template should only contain lowercase letters, digits, periods, underscores, or dashes.");

            if (splitTag.Length == 1)
                return (image, hasDigest, "{v}");

            if (splitTag.Length != 2)
                throw new FormatException("The image name for a template should only have one colon.");

            return (image, hasDigest, splitTag[1]);
        }

        private const string DefaultLibraryName = "library/";

        private string ImageString()
        {
            var sb = new StringBuilder();

            if (Repository == DefaultRepository && !_image.Contains("/") && !_image.StartsWith("library/"))
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

            if (Repository != DefaultRepository)
            {
                sb.Append(Repository.Host).Append('/');
            }

            sb.Append(Image);
                
            if (HasDigest)
            {
                sb.Append("@{digest}");
            }

            sb.Append(':').Append(Tag);

            return sb.ToString();
        }

        public string ToRepositoryImageString()
        {
            var sb = new StringBuilder();

            if (Repository != DefaultRepository)
            {
                sb.Append(Repository.Host).Append('/');
            }

            sb.Append(Image);

            return sb.ToString();
        }

        public DockerImageTemplatePattern CreatePattern(bool includeRepository, bool includeImage, bool includeDigest, bool includeTag, bool matchAnyVersion) =>
            CreatePattern(includeRepository, includeImage, includeDigest, includeTag, matchAnyVersion, null);

        public DockerImageTemplatePattern CreatePattern(bool includeRepository, bool includeImage, bool includeDigest, bool includeTag, bool matchAnyVersion, string? group)
        {
            var sb = new StringBuilder();

            if (Repository != DefaultRepository && includeRepository)
            {
                sb.Append(Repository.Host).Append('/');
            }

            var canIncludeDigest = HasDigest && includeDigest;

            if (includeImage)
            {
                sb.Append(_image);
            }

            if (canIncludeDigest)
            {
                if (includeImage)
                {
                    sb.Append('@');
                }

                sb.Append("{digest}");
            }
            
            if (!canIncludeDigest || includeTag)
            {
                if (includeImage || canIncludeDigest)
                {
                    sb.Append(':');
                }

                foreach (var part in _parts)
                {
                    sb.Append(part is FloatRange range ? (matchAnyVersion ? "{v}" : $"{{v{range}}}") : part);
                }
            }

            var pattern = sb.ToString();

            return DockerImageTemplatePattern.Parse(pattern, group ?? CreateDefaultGroup(), this);
        }

        public DockerImageTemplatePattern CreatePattern(string pattern) => CreatePattern(pattern, null);

        public DockerImageTemplatePattern CreatePattern(string pattern, string? group) => DockerImageTemplatePattern.Parse(pattern, group ?? CreateDefaultGroup(), this);

        private string CreateDefaultGroup()
        {
            var sb = new StringBuilder()
                .Append(Repository.Host)
                .Append('/')
                .Append(ImageString()).Append(':');

            foreach (var part in _parts)
            {
                sb.Append(part is FloatRange ? "{v}" : part);
            }

            return sb.ToString();
        }

        public override int GetHashCode() => HashCode.Combine(Repository, Image, Tag);

        public override bool Equals(object? obj)
        {
            if (obj is not DockerImageTemplate template)
                return false;

            return Equals(Repository, template.Repository) && Equals(Image, template.Image) && Equals(Tag, template.Tag);
        }
    }
}
