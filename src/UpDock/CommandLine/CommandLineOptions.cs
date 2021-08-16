using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using UpDock.Imaging;

namespace UpDock.CommandLine
{
    [Description("Automatically update docker images in github repositories.")]
    public class CommandLineOptions
    {
        [Shortcut("e", "email")]
        [Description("Email to use in the commit")]
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Shortcut("t", "token")]
        [Description("GitHub token to access the repository")]
        public string? Token { get; set; }

        [Shortcut("s", "search")]
        [Description("Search query to get repositories")]
        [Required]
        [ValidSearchFormat]
        public string? Search { get; set; }

        [Shortcut("c", "config")]
        [Description("Default configuration to apply")]
        [ValidFilePath]
        [ValidJsonFile]
        public string? Config { get; set; }

        [Shortcut("i", "template")]
        [Description("A template to apply")]
        [TypeConverter(typeof(DockerImageTemplatePatternConverter))]
        public DockerImageTemplatePattern[] Templates { get; set; } = Array.Empty<DockerImageTemplatePattern>();

        [Shortcut("a", "auth")]
        [Description("Authentication for a repository")]
        [ValidAuthenticationFormat]
        public string[] Authentication { get; set; } = Array.Empty<string>();

        [Shortcut("d", "dry-run")]
        [Description("Run without creating pull requests")]
        public bool DryRun { get; set; }

        [Shortcut("h", "help")]
        [Description("Display help information")]
        public bool Help { get; set; }

        [Shortcut("v", "version")]
        [Description("Display what the version is")]
        public bool Version { get; set; }

        [Shortcut("l", "allow-downgrade")]
        [Description("Allow downgrading if the version is higher than the one specified")]
        public bool AllowDowngrade { get; set; }

        [Shortcut("cache")]
        [Description("Cache the results from this run to re-use in another")]
        public string? Cache { get; set; }

        [Shortcut("r", "report")]
        [Description("Output a report to a file on the pull requests that were created")]
        public string? Report { get; set; }

        [Shortcut("f", "fork-only")]
        [Description("Should only create pull requests using forks")]
        public bool ForkOnly { get; set; }
    }
}
