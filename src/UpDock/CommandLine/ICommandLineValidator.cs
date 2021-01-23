using System.Collections.Generic;

namespace UpDock.CommandLine
{
    public interface ICommandLineValidator
    {
        void Validate<T>(IReadOnlyList<CommandLineArgument> arguments);
    }
}
