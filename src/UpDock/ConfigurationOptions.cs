using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UpDock.Imaging;

namespace UpDock
{
    public class ConfigurationOptions : IConfigurationOptions
    {
        private readonly HashSet<string> _include = new();
        private readonly HashSet<string> _exclude = new();
        private readonly HashSet<DockerImageTemplatePattern> _patterns = new();
        private readonly Dictionary<string, AuthenticationOptions> _authentication = new();
        public ICollection<string> Include => _include;
        public ICollection<string> Exclude => _exclude;
        public ICollection<DockerImageTemplatePattern> Patterns => _patterns;
        public string? Search { get; set; }
        public string? Token { get; set; }
        public bool DryRun { get; set; }
        public bool AllowDowngrade { get; set; }
        public IDictionary<string, AuthenticationOptions> Authentication => _authentication;

        IReadOnlyCollection<string> IConfigurationOptions.Include => _include;
        IReadOnlyCollection<string> IConfigurationOptions.Exclude => _exclude;
        IReadOnlyCollection<DockerImageTemplatePattern> IConfigurationOptions.Patterns => _patterns;
        IReadOnlyDictionary<string, AuthenticationOptions> IConfigurationOptions.Authentication => _authentication;
        string? IConfigurationOptions.Search => Search;
        string? IConfigurationOptions.Token => Token;
        bool IConfigurationOptions.DryRun => DryRun;
        bool IConfigurationOptions.AllowDowngrade => AllowDowngrade;

        public void Populate(Stream stream)
        {
            var doc = JsonDocument.Parse(stream);

            PopulateIncludeExclude(_include, doc, "include");
            PopulateIncludeExclude(_exclude, doc, "exclude");

            if (doc.RootElement.TryGetProperty("templates", out var templates))
            {
                if (templates.ValueKind != JsonValueKind.Array)
                    throw new InvalidOperationException("Invalid configuration file");

                foreach (var template in templates.EnumerateArray())
                {
                    _patterns.Add(ParsePattern(template));
                }
            }
        }

        private static void PopulateIncludeExclude(ISet<string> list, JsonDocument doc, string name)
        {
            if (!doc.RootElement.TryGetProperty(name, out var element))
                return;

            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    list.Add(element.GetString()!);
                    break;
                case JsonValueKind.Array:
                    foreach (var subElement in element.EnumerateArray())
                    {
                        list.Add(subElement.GetString()!);
                    }
                    break;
                default:
                    throw new InvalidOperationException("Invalid configuration file");
            }
        }

        private static DockerImageTemplatePattern ParsePattern(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return ParsePattern(element.GetString()!);
                case JsonValueKind.Object:
                {
                    var image = GetRequiredJsonProperty(element, "image");

                    var pattern = GetOptionalProperty(element, "pattern");

                    var group = GetOptionalProperty(element, "group");

                    var template = DockerImageTemplate.Parse(image);

                    return pattern == null ? template.CreatePattern(true, true, true, group) : template.CreatePattern(pattern, group);
                }
                default:
                    throw new InvalidOperationException("Invalid configuration file");
            }
        }

        private static string GetRequiredJsonProperty(JsonElement element, string name)
        {
            if (!element.TryGetProperty(name, out var childElement))
                throw new InvalidOperationException($"Invalid configuration file: expected {name} was not found");

            return GetStringProperty(childElement, name);
        }

        private static string? GetOptionalProperty(JsonElement element, string name)
        {
            if (!element.TryGetProperty(name, out var childElement))
                return null;

            return GetStringProperty(childElement, name);
        }

        private static string GetStringProperty(JsonElement element, string name)
        {
            if (element.ValueKind != JsonValueKind.String)
                throw new InvalidOperationException($"Invalid configuration file: expected {name} to be a string");

            return element.GetString()!;
        }

        public static DockerImageTemplatePattern ParsePattern(string pattern) => DockerImageTemplate.Parse(pattern).CreatePattern(true, true, true, null);

        public IConfigurationOptions Merge(IConfigurationOptions options)
        {
            var newOptions = new ConfigurationOptions();

            foreach (var include in Include.Concat(options.Include).Distinct())
            {
                newOptions.Include.Add(include);
            }

            foreach (var exclude in Exclude.Concat(options.Exclude).Distinct())
            {
                newOptions.Exclude.Add(exclude);
            }

            foreach (var pattern in Patterns.Concat(options.Patterns).Distinct())
            {
                newOptions.Patterns.Add(pattern);
            }

            return newOptions;
        }

        private static readonly SHA256 Sha256Hash = SHA256.Create();

        public string CreateHash()
        {
            var builder = new StringBuilder();

            builder.Append(':');

            foreach (var include in Include)
            {
                builder.AppendLine(include);
            }

            builder.Append(':');

            foreach (var exclude in Exclude)
            {
                builder.AppendLine(exclude);
            }

            builder.Append(':').Append(DryRun).Append(':').Append(AllowDowngrade).Append(':');

            foreach(var pattern in Patterns)
            {
                builder
                    .Append(pattern.Group)
                    .Append(':')
                    .Append(pattern.ToString())
                    .Append(':')
                    .Append(pattern.Template.ToString());
            }

            return Sha256Hash.ComputeHash(builder.ToString());
        }
    }
}
