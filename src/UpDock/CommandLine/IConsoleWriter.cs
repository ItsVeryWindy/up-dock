namespace UpDock.CommandLine
{
    public interface IConsoleWriter
    {
        IConsoleWriter WriteLine(string? str);
        IConsoleWriter WriteLine();
    }
}
