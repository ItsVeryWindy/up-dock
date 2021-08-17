using UpDock.Git.Drivers;

namespace UpDock.Git
{
    public interface IGitDriver
    {
        IRepository Clone(string cloneUrl, string dir, string? token);
    }
}
