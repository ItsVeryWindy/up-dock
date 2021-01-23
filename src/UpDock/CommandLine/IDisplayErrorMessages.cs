using System.Collections.Generic;

namespace UpDock.CommandLine
{
    public interface IDisplayErrorMessages
    {
        void Display(IReadOnlyList<CommandLineArgument> arguments);
    }
}
