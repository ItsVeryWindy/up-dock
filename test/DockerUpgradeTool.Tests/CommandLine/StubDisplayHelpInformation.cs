using DockerUpgradeTool.CommandLine;

namespace DockerUpgradeTool.Tests.CommandLine
{
    internal class StubDisplayHelpInformation : IDisplayHelpInformation
    {
        public bool WasCalled { get; private set; }

        public void Display<T>() => WasCalled = true;
    }
}
