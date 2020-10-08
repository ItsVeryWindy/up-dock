using DockerUpgradeTool.Imaging;
using PowerArgs;

namespace DockerUpgradeTool.CommandLine
{
    public class CommandLineOptions
    {
        [ArgShortcut("-e"), ArgShortcut("--email"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgDescription("Email to use in the commit")]
        [ArgRequired]
        public string? Email { get; set; }

        [ArgShortcut("-t"), ArgShortcut("--token"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgDescription("GitHub token to access the repository")]
        public string? Token { get; set; }

        [ArgShortcut("-s"), ArgShortcut("--search"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgDescription("Search query to get repositories")]
        [ArgRequired]
        public string? Search { get; set; }

        [ArgShortcut("-c"), ArgShortcut("--config"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgDescription("Default configuration to apply")]
        public string? Config { get; set; }

        [ArgShortcut("-i"), ArgShortcut("--template"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgDescription("A template to apply")]
        public DockerImageTemplatePattern[] Templates { get; set; } = new DockerImageTemplatePattern[0];

        [ArgShortcut("-a"), ArgShortcut("--auth"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgDescription("Authentication for a repository")]
        [AuthenticationArgValidator]
        public string[] Authentication { get; set; } = new string[0];

        [ArgShortcut("-d"), ArgShortcut("--dry-run"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgDescription("Run without creating pull requests")]
        public bool DryRun { get; set; }

        [ArgShortcut("-h"), ArgShortcut("--help"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgDescription("Display help information")]
        public bool Help { get; set; }

        [ArgShortcut("-v"), ArgShortcut("--version"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgDescription("Run without creating pull requests")]
        public bool Version { get; set; }
    }
}
