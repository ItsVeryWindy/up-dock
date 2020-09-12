using System.Threading;

namespace DockerUpgrader
{
    public class CancellationProvider : ICancellationProvider
    {
        public CancellationToken CancellationToken { get; }

        public CancellationProvider(in CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }
    }
}