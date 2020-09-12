using DockerUpgradeTool.Files;
using DockerUpgradeTool.Git;

namespace DockerUpgradeTool
{
    public interface IFileFilter
    {
        bool Filter(ILocalGitRepository repository, IFileInfo file);
    }
}
