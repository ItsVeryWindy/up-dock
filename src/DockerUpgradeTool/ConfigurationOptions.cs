﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DockerUpgradeTool.Imaging;

namespace DockerUpgradeTool
{
    public class ConfigurationOptions : IConfigurationOptions
    {
        private readonly HashSet<string> _include = new HashSet<string>();
        private readonly HashSet<string> _exclude = new HashSet<string>();
        private readonly HashSet<DockerImageTemplatePattern> _patterns = new HashSet<DockerImageTemplatePattern>();
        private readonly Dictionary<string, AuthenticationOptions> _authentication = new Dictionary<string, AuthenticationOptions>();
        public ICollection<string> Include => _include;
        public ICollection<string> Exclude => _exclude;
        public ICollection<DockerImageTemplatePattern> Patterns => _patterns;
        public string? Search { get; set; }
        public string? Token { get; set; }
        public bool DryRun { get; set; }
        public IDictionary<string, AuthenticationOptions> Authentication => _authentication;

        IReadOnlyCollection<string> IConfigurationOptions.Include => _include;
        IReadOnlyCollection<string> IConfigurationOptions.Exclude => _exclude;
        IReadOnlyCollection<DockerImageTemplatePattern> IConfigurationOptions.Patterns => _patterns;
        IReadOnlyDictionary<string, AuthenticationOptions> IConfigurationOptions.Authentication => _authentication;
        string? IConfigurationOptions.Search => Search;
        string? IConfigurationOptions.Token => Token;
        bool IConfigurationOptions.DryRun => DryRun;

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
                    list.Add(element.GetString());
                    break;
                case JsonValueKind.Array:
                    foreach (var subElement in element.EnumerateArray())
                    {
                        list.Add(subElement.GetString());
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
                    return ParsePattern(element.GetString());
                case JsonValueKind.Object:
                {
                    var repository = element.GetProperty("repository").GetString();

                    var image = element.GetProperty("image").GetString();

                    var pattern = element.TryGetProperty("pattern", out var patternElement) ? patternElement.GetString() : null;

                    var group = element.TryGetProperty("group", out var groupElement) ? groupElement.GetString() : null;

                    var template = DockerImageTemplate.Parse(image, new Uri($"https://{repository}"));

                    return pattern == null ? template.CreatePattern(false, true, group) : template.CreatePattern(pattern, group);
                }
                default:
                    throw new InvalidOperationException("Invalid configuration file");
            }
        }

        public static DockerImageTemplatePattern ParsePattern(string pattern) => DockerImageTemplate.Parse(pattern).CreatePattern(true, true, null);

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
    }
}
