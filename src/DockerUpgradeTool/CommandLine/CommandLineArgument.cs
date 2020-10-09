using System.Collections.Generic;
using System.Reflection;

namespace DockerUpgradeTool.CommandLine
{
    public class CommandLineArgument
    {
        public string Argument { get; }
        public PropertyInfo? Property { get; }
        public string? OriginalValue { get; }
        public object? Value { get; }
        public int Index { get; }

        public List<string> Errors { get; } = new List<string>();

        public CommandLineArgument(string argument, string? originalValue, object? value, PropertyInfo? property, int index)
        {
            Argument = argument;
            OriginalValue = originalValue;
            Value = value;
            Property = property;
            Index = index;
        }
    }
}
