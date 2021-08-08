using System.Collections.Generic;
using UpDock.CommandLine;

namespace UpDock.Tests.CommandLine
{
    internal class StubConsoleWriter : IConsoleWriter
    {
        private readonly List<string?> _lines = new();

        public IReadOnlyList<string?> Lines => _lines;

        public IConsoleWriter WriteLine(string? str)
        {
            _lines.Add(str);

            return this;
        }

        public IConsoleWriter WriteLine()
        {
            _lines.Add(null);

            return this;
        }
    }
}
