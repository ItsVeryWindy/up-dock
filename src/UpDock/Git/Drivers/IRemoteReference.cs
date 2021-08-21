using System.Threading;
using System.Threading.Tasks;

namespace UpDock.Git.Drivers
{
    public interface IRemoteReference
    {
        string FullName { get; }

        Task RemoveAsync(CancellationToken cancellationToken);
    }
}
