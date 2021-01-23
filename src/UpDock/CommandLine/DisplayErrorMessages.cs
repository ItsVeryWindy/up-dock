using System.Collections.Generic;

namespace UpDock.CommandLine
{
    public class DisplayErrorMessages : IDisplayErrorMessages
    {
        private readonly IConsoleWriter _writer;

        public DisplayErrorMessages(IConsoleWriter writer)
        {
            _writer = writer;
        }

        public void Display(IReadOnlyList<CommandLineArgument> arguments)
        {
            foreach(var argument in arguments)
            {
                if (argument.Errors.Count == 0)
                    continue;

                _writer.WriteLine($"{argument.Argument}: ({argument.OriginalValue})");

                foreach (var error in argument.Errors)
                {
                    _writer.WriteLine($"{new string(' ', argument.Argument.Length)}  {error}");
                }
            }

            _writer.WriteLine();
        }
    }
}
