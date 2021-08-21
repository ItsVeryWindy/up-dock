using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UpDock.Git.Drivers
{
    public interface IRemote
    {
        Task<IEnumerable<IRemoteReference>> GetReferencesAsync(CancellationToken cancellationToken);
        string Name { get; }
    }
}
