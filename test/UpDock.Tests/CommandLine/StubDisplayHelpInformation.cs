using UpDock.CommandLine;

namespace UpDock.Tests.CommandLine
{
    internal class StubDisplayHelpInformation : IDisplayHelpInformation
    {
        public bool WasCalled { get; private set; }

        public void Display<T>() => WasCalled = true;
    }
}
