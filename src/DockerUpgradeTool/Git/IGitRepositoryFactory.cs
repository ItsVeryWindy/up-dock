using Octokit;

namespace DockerUpgradeTool.Git
{
    public interface IGitRepositoryFactory
    {
        IRemoteGitRepository CreateRepository(Repository repository);
    }
}
