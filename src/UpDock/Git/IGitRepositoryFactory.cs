using Octokit;

namespace UpDock.Git
{
    public interface IGitRepositoryFactory
    {
        IRemoteGitRepository CreateRepository(Repository repository);
    }
}
