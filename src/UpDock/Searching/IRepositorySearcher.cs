using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Git;

namespace UpDock
{
    public interface IRepositorySearcher
    {
        Task<IEnumerable<IRemoteGitRepository>> SearchAsync(string search, CancellationToken cancellationToken);
    }
}
