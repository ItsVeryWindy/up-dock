using Octokit;

namespace DockerUpgrader.Git
{
    public interface IGitRepositoryFactory
    {
        IRemoteGitRepository CreateRepository(Repository repository);
    }
}
