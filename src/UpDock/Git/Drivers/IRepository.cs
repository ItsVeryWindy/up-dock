using System.Collections.Generic;
using UpDock.Files;

namespace UpDock.Git.Drivers
{
    public interface IRepository
    {
        bool IsDirty { get; }
        IBranch Head { get; }
        IEnumerable<IBranch> Branches { get; }
        IEnumerable<IRepositoryFileInfo> Files { get; }
        IDirectoryInfo Directory { get; }
        IEnumerable<IRemote> Remotes { get; }

        IBranch CreateBranch(string name);

        void Commit(string message, string email);

        IRemote CreateRemote(string remoteName, IRemoteGitRepository repository);
    }
}
