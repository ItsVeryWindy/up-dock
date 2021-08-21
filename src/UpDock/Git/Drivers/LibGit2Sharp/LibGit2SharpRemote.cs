using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace UpDock.Git.Drivers
{
    public class LibGit2SharpRemote : IRemote
    {
        private readonly Repository _repository;
        private readonly Remote _remote;
        private readonly CredentialsHandler _credentialsHandler;
        private IEnumerable<Reference>? _references;

        public Task<IEnumerable<IRemoteReference>> GetReferencesAsync(CancellationToken cancellationToken)
        {
            _references ??= _repository.Network.ListReferences(_remote, _credentialsHandler);

            return Task.FromResult<IEnumerable<IRemoteReference>>(_references.Where(x => x.CanonicalName != "HEAD").Select(x => new LibGit2SharpRemoteReference(_repository, _remote, x, _credentialsHandler)));
        }

        public string Name => _remote.Name;

        public LibGit2SharpRemote(Repository repository, Remote remote, CredentialsHandler credentialsHandler)
        {
            _repository = repository;
            _remote = remote;
            _credentialsHandler = credentialsHandler;
        }
    }
}
