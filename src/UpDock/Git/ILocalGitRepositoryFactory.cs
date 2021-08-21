using System.Threading;
using System.Threading.Tasks;

namespace UpDock.Git
{
    public interface ILocalGitRepositoryFactory
    {
        Task<ILocalGitRepository> CreateAsync(string cloneUrl, string dir, IRemoteGitRepository remoteGitRepository, CancellationToken cancellationToken);
    }
}
