using System;
using System.Collections.Generic;
using System.IO;

namespace UpDock.CommandLine
{
    public interface ICommandLineParser
    {
        IReadOnlyList<CommandLineArgument> Parse<T>(string[] args) => Parse<T>(args, Console.IsInputRedirected ? Console.In : StreamReader.Null);
        IReadOnlyList<CommandLineArgument> Parse<T>(string[] args, TextReader input);
    }
}
