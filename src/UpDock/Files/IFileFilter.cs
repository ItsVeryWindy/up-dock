using System.Threading;
using System.Threading.Tasks;
using UpDock.Git;

namespace UpDock
{
    public interface IFileFilter
    {
        Task<bool> FilterAsync(IRepositoryFileInfo file, CancellationToken cancellationToken);
    }
}
