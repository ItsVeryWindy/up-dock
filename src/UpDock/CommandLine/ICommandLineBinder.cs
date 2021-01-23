using System.Collections.Generic;

namespace UpDock.CommandLine
{
    public interface ICommandLineBinder
    {
        void Bind<T>(IReadOnlyList<CommandLineArgument> arguments, T options);
    }
}
