using System.Collections.Generic;

namespace DockerUpgradeTool.CommandLine
{
    public interface ICommandLineValidator
    {
        void Validate<T>(IReadOnlyList<CommandLineArgument> arguments);
    }
}
