using System.Threading;
using System.Threading.Tasks;

namespace UpDock.Git.Drivers
{
    public interface IRemoteBranch : IBranch
    {
        Task PushAsync(CancellationToken cancellationToken);
    }
}
