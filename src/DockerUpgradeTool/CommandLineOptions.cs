using CommandLine;

namespace DockerUpgradeTool
{
    public class CommandLineOptions
    {
        [Option('e', "email", Required = true, HelpText = "Email to use in the commit")]
        public string? Email { get; set; }

        [Option('t', "token", Required = true, HelpText = "GitHub token to access the repository")]
        public string? Token { get; set; }

        [Option('s', "search", Required = true, HelpText = "Search query to get repositories")]
        public string? Search { get; set; }

        [Option('c', "config", Required = false, HelpText = "Default configuration to apply")]
        public string? Config { get; set; }

        [Option('t', "template", Required = false, HelpText = "Default configuration to apply")]
        public string[]? Templates { get; set; }

        [Option('a', "auth", Required = false, HelpText = "Authentication for a repository")]
        public string[]? Authentication { get; set; }
    }
}
