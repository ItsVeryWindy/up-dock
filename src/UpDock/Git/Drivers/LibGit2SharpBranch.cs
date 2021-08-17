using System;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace UpDock.Git.Drivers
{
    public class LibGit2SharpBranch : IBranch, IRemoteBranch
    {
        private readonly Repository _repository;
        private readonly Branch _branch;
        private readonly Remote? _remote;
        private readonly CredentialsHandler _credentialsHandler;

        public string Name => _branch.FriendlyName;

        public string FullName => _branch.CanonicalName;

        public bool IsRemote => _branch.IsRemote;

        public LibGit2SharpBranch(Repository repository, Branch branch, Remote? remote, CredentialsHandler credentialsHandler)
        {
            _repository = repository;
            _branch = branch;
            _remote = remote;
            _credentialsHandler = credentialsHandler;
        }

        public void Checkout() => Checkout(false);

        public void Checkout(bool force)
        {
            Commands.Checkout(_repository, _branch, new CheckoutOptions
            {
                CheckoutModifiers = force ? CheckoutModifiers.Force : CheckoutModifiers.None
            });
        }

        public void Push()
        {
            var pushRefSpec = string.Format("+{0}:{0}", _branch.CanonicalName);

            PushException? exception = null;

            _repository.Network.Push(_remote, pushRefSpec, new PushOptions
            {
                CredentialsProvider = _credentialsHandler,
                OnPushStatusError = error => {
                    exception = new PushException(error.Reference, error.Message);
                }
            });

            if (exception is not null)
                throw exception;
        }

        public IRemoteBranch Track(IRemote remote)
        {
            _repository.Branches.Update(_branch,
                b => b.Remote = remote.Name,
                b => b.UpstreamBranch = _branch.CanonicalName);

            return new LibGit2SharpBranch(_repository, _branch, _remote, _credentialsHandler);
        }
    }
}
