namespace UpDock.Git
{
    public interface ILocalGitRepositoryFactory
    {
        ILocalGitRepository Create(string cloneUrl, string dir, IRemoteGitRepository remoteGitRepository);
    }
}
