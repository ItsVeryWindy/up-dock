using System.Collections.Generic;

namespace DockerUpgradeTool.CommandLine
{
    public interface ICommandLineBinder
    {
        void Bind<T>(IReadOnlyList<CommandLineArgument> arguments, T options);
    }
}
