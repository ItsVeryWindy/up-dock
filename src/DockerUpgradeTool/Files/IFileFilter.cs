using DockerUpgradeTool.Git;

namespace DockerUpgradeTool
{
    public interface IFileFilter
    {
        bool Filter(IRepositoryFileInfo file);
    }
}
