using System.Threading;

namespace DockerUpgradeTool
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
