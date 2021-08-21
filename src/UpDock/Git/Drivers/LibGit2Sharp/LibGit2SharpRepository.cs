using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using UpDock.Files;

namespace UpDock.Git.Drivers
{
    public class LibGit2SharpRepository : IRepository
    {
        private readonly Repository _repository;
        private readonly IFileProvider _provider;
        private readonly CredentialsHandler _credentialsHandler;

        public LibGit2SharpRepository(Repository repository, IFileProvider provider, CredentialsHandler credentialsHandler)
        {
            _repository = repository;
            _provider = provider;
            _credentialsHandler = credentialsHandler;
        }

        public Task<bool> IsDirtyAsync(CancellationToken cancellationToken) => Task.FromResult(_repository.RetrieveStatus(new StatusOptions()).IsDirty);

        public IEnumerable<IRepositoryFileInfo> Files => Directory.AllFiles.Select(x => new RepositoryFileInfo(this, x));

        public IDirectoryInfo Directory => _provider.GetDirectory(_repository.Info.WorkingDirectory);

        public Task CommitAsync(string message, string email, CancellationToken cancellationToken)
        {
            var author = new Signature("up-dock", email, DateTime.Now);

            _repository.Commit(message, author, author);

            return Task.CompletedTask;
        }

        public Task<IRemote> CreateRemoteAsync(string name, IRemoteGitRepository repository, CancellationToken cancellationToken)
        {
            var newRemote = _repository.Network.Remotes.Add(name, repository.CloneUrl);

            return Task.FromResult<IRemote>(new LibGit2SharpRemote(_repository, newRemote, _credentialsHandler));
        }

        public Task<IReadOnlyCollection<IRemote>> GetRemotesAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<IRemote>>(_repository.Network.Remotes.Select(x => new LibGit2SharpRemote(_repository, x, _credentialsHandler)).ToList());
        public Task<IBranch> GetHeadAsync(CancellationToken none) => Task.FromResult<IBranch>(new LibGit2SharpBranch(_repository, _repository.Head, null, _credentialsHandler));
        public Task<IReadOnlyCollection<IBranch>> GetBranchesAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<IBranch>>(_repository.Branches.Select(x => new LibGit2SharpBranch(_repository, x, null, _credentialsHandler)).ToList());
        public Task<IBranch> CreateBranchAsync(string name, CancellationToken cancellationToken) => Task.FromResult<IBranch>(new LibGit2SharpBranch(_repository, _repository.CreateBranch(name), null, _credentialsHandler));

        private class RepositoryFileInfo : IRepositoryFileInfo
        {
            private readonly LibGit2SharpRepository _repository;

            public RepositoryFileInfo(LibGit2SharpRepository repository, IFileInfo file)
            {
                _repository = repository;
                File = file;
            }

            public IFileInfo File { get; }

            public bool Ignored => _repository._repository.Ignore.IsPathIgnored(RelativePath);

            public string RelativePath => Path.GetRelativePath(_repository._repository.Info.WorkingDirectory, File.AbsolutePath);

            public IDirectoryInfo Root => _repository.Directory;

            public Task<bool> IsIgnoredAsync(CancellationToken cancellationToken) => Task.FromResult(_repository._repository.Ignore.IsPathIgnored(RelativePath));

            public Task StageAsync(CancellationToken cancellationToken)
            {
                Commands.Stage(_repository._repository, RelativePath);

                return Task.CompletedTask;
            }
        }
    }
}
