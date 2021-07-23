namespace UpDock.Git
{
    public interface IGitRepositoryFactory
    {
        IRemoteGitRepository CreateRepository(IRepository repository);
    }
}
