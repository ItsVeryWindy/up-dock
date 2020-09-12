using System.Threading;

namespace DockerUpgradeTool
{
    public interface ICancellationProvider
    {
        CancellationToken CancellationToken { get; }
    }
}
