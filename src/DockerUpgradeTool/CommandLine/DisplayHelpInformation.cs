using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DockerUpgradeTool.CommandLine
{
    public class DisplayHelpInformation : IDisplayHelpInformation
    {
        private readonly IConsoleWriter _writer;
        private readonly IProcessInfo _processInfo;

        public DisplayHelpInformation(IConsoleWriter writer, IProcessInfo processInfo)
        {
            _writer = writer;
            _processInfo = processInfo;
        }

        public void Display<T>()
        {
            var description = typeof(T).GetCustomAttribute<DescriptionAttribute>()?.Description;

            _writer
                .WriteLine($"Usage: {_processInfo.Name} [OPTIONS]")
                .WriteLine()
                .WriteLine(description)
                .WriteLine()
                .WriteLine("Options:");

            var shortcuts = typeof(T)
                .GetProperties()
                .Select(x => new {
                    shortcuts = Formatter.FormatShortcut(x) + (x.GetCustomAttribute<RequiredAttribute>() == null ? "" : "*"),
                    description = x.GetCustomAttribute<DescriptionAttribute>()?.Description
                });

            var maxLength = shortcuts.Select(x => x.shortcuts.Length).Max() + 3;

            foreach (var shortcut in shortcuts)
            {
                _writer.WriteLine($"{shortcut.shortcuts.PadRight(maxLength)} {shortcut.description}");
            }
        }
    }
}
