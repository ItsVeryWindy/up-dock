namespace DockerUpgradeTool.CommandLine
{
    public interface IProcessInfo
    {
        string Name { get; }
        string Version { get; }
    }
}
