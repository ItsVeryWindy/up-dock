using System.Collections.Generic;

namespace DockerUpgradeTool.CommandLine
{
    public interface IDisplayErrorMessages
    {
        void Display(IReadOnlyList<CommandLineArgument> arguments);
    }
}
