using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpDock.Files;
using UpDock.Git;
using UpDock.Git.Drivers;

namespace UpDock.Tests
{
    class StubGitDriver : IGitDriver
    {
        public IRepository Clone(string cloneUrl, string dir, string? token) => throw new NotImplementedException();

        class StubRepository : IRepository
        {
            public bool IsDirty => throw new NotImplementedException();

            public IBranch Head => throw new NotImplementedException();

            public IEnumerable<IBranch> Branches => throw new NotImplementedException();

            public IEnumerable<IRepositoryFileInfo> Files => throw new NotImplementedException();

            public IDirectoryInfo Directory => throw new NotImplementedException();

            public IEnumerable<IRemote> Remotes => throw new NotImplementedException();

            public void Commit(string message, string email) => throw new NotImplementedException();
            public IBranch CreateBranch(string name) => throw new NotImplementedException();
            public IRemote CreateRemote(string remoteName, IRemoteGitRepository repository) => throw new NotImplementedException();
        }
    }
}
