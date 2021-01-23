using UpDock.CommandLine;

namespace UpDock.Tests.CommandLine
{
    internal class StubProcessInfo : IProcessInfo
    {
        public string Name => "ProcessName";

        public string Version => "ProcessVersion";
    }
}
