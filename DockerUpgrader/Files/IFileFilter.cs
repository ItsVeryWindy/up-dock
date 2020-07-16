using DockerUpgrader.Files;
using DockerUpgrader.Git;

namespace DockerUpgrader
{
    public interface IFileFilter
    {
        bool Filter(ILocalGitRepository repository, IFileInfo file);
    }
}
