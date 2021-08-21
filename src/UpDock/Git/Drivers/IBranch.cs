using System.Threading;
using System.Threading.Tasks;

namespace UpDock.Git.Drivers
{
    public interface IBranch
    {
        string Name { get; }
        string FullName { get; }
        bool IsRemote { get; }

        Task CheckoutAsync(CancellationToken cancellationToken) => CheckoutAsync(false, cancellationToken);
        Task CheckoutAsync(bool force, CancellationToken cancellationToken);
        Task<IRemoteBranch> TrackAsync(IRemote remote, CancellationToken cancellationToken);
    }
}
