namespace UpDock.CommandLine
{
    public class ProcessInfo : IProcessInfo
    {
        public string Name { get; }
        public string Version { get; }

        public ProcessInfo(string name, string version)
        {
            Name = name;
            Version = version;
        }
    }
}
