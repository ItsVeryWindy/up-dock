using UpDock.Git;

namespace UpDock
{
    public interface IFileFilter
    {
        bool Filter(IRepositoryFileInfo file);
    }
}
