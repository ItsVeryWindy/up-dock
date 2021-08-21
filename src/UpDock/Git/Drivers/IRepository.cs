using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Files;

namespace UpDock.Git.Drivers
{
    public interface IRepository
    {
        Task<bool> IsDirtyAsync(CancellationToken cancellationToken);
        IEnumerable<IRepositoryFileInfo> Files { get; }
        IDirectoryInfo Directory { get; }
        Task<IReadOnlyCollection<IRemote>> GetRemotesAsync(CancellationToken cancellationToken);
        Task<IReadOnlyCollection<IBranch>> GetBranchesAsync(CancellationToken cancellationToken);
        Task<IBranch> CreateBranchAsync(string name, CancellationToken cancellationToken);
        Task CommitAsync(string message, string email, CancellationToken cancellationToken);
        Task<IRemote> CreateRemoteAsync(string name, IRemoteGitRepository repository, CancellationToken cancellationToken);
        Task<IBranch> GetHeadAsync(CancellationToken none);
    }
}
