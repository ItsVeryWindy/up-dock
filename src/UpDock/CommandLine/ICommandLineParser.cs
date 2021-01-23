using System.Collections.Generic;

namespace UpDock.CommandLine
{
    public interface ICommandLineParser
    {
        IReadOnlyList<CommandLineArgument> Parse<T>(string[] args);
    }
}
