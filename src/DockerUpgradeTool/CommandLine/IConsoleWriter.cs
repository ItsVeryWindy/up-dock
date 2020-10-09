namespace DockerUpgradeTool.CommandLine
{
    public interface IConsoleWriter
    {
        IConsoleWriter WriteLine(string? str);
        IConsoleWriter WriteLine();
    }
}
