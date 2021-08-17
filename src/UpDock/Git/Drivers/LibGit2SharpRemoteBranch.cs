using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace UpDock.Git.Drivers
{
    public class LibGit2SharpRemoteReference : IRemoteReference
    {
        private readonly Repository _repository;
        private readonly Reference _reference;
        private readonly Remote _remote;
        private readonly CredentialsHandler _credentialsHandler;

        public string FullName => _reference.CanonicalName;

        public LibGit2SharpRemoteReference(Repository repository, Remote remote, Reference reference, CredentialsHandler credentialsHandler)
        {
            _repository = repository;
            _remote = remote;
            _reference = reference;
            _credentialsHandler = credentialsHandler;
        }

        public void Remove()
        {
            PushException? exception = null;

            _repository.Network.Push(_remote, $"+:{_reference.CanonicalName}", new PushOptions
            {
                CredentialsProvider = _credentialsHandler,
                OnPushStatusError = error => {
                    exception = new PushException(error.Reference, error.Message);
                }
            });

            if (exception is not null)
                throw exception;
        }
    }
}
