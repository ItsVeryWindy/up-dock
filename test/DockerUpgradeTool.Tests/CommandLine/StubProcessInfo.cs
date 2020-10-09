using DockerUpgradeTool.CommandLine;

namespace DockerUpgradeTool.Tests.CommandLine
{
    internal class StubProcessInfo : IProcessInfo
    {
        public string Name => "ProcessName";

        public string Version => "ProcessVersion";
    }
}
