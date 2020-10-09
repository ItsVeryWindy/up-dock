using System.Collections.Generic;

namespace DockerUpgradeTool.CommandLine
{
    public interface ICommandLineParser
    {
        IReadOnlyList<CommandLineArgument> Parse<T>(string[] args);
    }
}
