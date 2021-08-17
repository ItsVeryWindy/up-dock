using System.Collections.Generic;

namespace UpDock.Git.Drivers
{
    public interface IRemote
    {
        IEnumerable<IRemoteReference> Branches { get; }
        string Name { get; }
    }
}
