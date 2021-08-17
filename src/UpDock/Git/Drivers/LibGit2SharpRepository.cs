using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public bool IsDirty => _repository.RetrieveStatus(new StatusOptions()).IsDirty;

        public IBranch Head => new LibGit2SharpBranch(_repository, _repository.Head, null, _credentialsHandler);

        public IEnumerable<IBranch> Branches => _repository.Branches.Select(x => new LibGit2SharpBranch(_repository, x, null, _credentialsHandler));

        public IEnumerable<IRepositoryFileInfo> Files => Directory.Files.Select(x => new RepositoryFileInfo(this, x));

        public IDirectoryInfo Directory => _provider.GetDirectory(_repository.Info.WorkingDirectory);

        public IEnumerable<IRemote> Remotes => _repository.Network.Remotes.Select(x => new LibGit2SharpRemote(_repository, x, _credentialsHandler));

        public void Commit(string message, string email)
        {
            var author = new Signature("up-dock", email, DateTime.Now);

            _repository.Commit(message, author, author);
        }

        public IBranch CreateBranch(string name) => new LibGit2SharpBranch(_repository, _repository.CreateBranch(name), null, _credentialsHandler);
        
        public IRemote CreateRemote(string name, IRemoteGitRepository repository)
        {
            var newRemote = _repository.Network.Remotes.Add(name, repository.CloneUrl);

            return new LibGit2SharpRemote(_repository, newRemote, _credentialsHandler);
        }

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

            public void Stage() => Commands.Stage(_repository._repository, RelativePath);
        }
    }
}
