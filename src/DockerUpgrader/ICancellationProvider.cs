using System.Threading;

namespace DockerUpgrader
{
    public interface ICancellationProvider
    {
        CancellationToken CancellationToken { get; }
    }
}
