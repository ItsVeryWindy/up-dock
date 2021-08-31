using System;
using System.Threading;
using System.Threading.Tasks;

namespace UpDock.Git.Drivers
{
    public class GitProcessBranch : IBranch, IRemoteBranch
    {
        private readonly GitProcess _process;
        private readonly GitProcessRepository _repository;
        private readonly GitProcessRemote? _remote;

        public GitProcessBranch(GitProcess process, GitProcessRepository repository, string fullName, string name, GitProcessRemote? remote)
        {
            _process = process;
            _repository = repository;
            FullName = fullName;
            Name = name;
            _remote = remote;
        }

        public string Name { get; }

        public string FullName { get; }

        public bool IsRemote => FullName.StartsWith("refs/remotes/");

        public async Task CheckoutAsync(bool force, CancellationToken cancellationToken)
        {
            var result = await (force ? _process.ExecuteAsync(cancellationToken, "checkout", "-f", Name) : _process.ExecuteAsync(cancellationToken, "checkout", Name));

            await result.EnsureSuccessExitCodeAsync();
        }

        public async Task PushAsync(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _process.ExecuteAsync(cancellationToken, "push", "-u", _remote!.Name, Name);

                await result.EnsureSuccessExitCodeAsync();
            }
            catch (Exception ex)
            {
                throw new PushException(FullName, ex);
            }
        }

        public async Task<IRemoteBranch> TrackAsync(IRemote remote, CancellationToken cancellationToken)
        {
            var result1 = await _process.ExecuteAsync(cancellationToken, "config", $"branch.{Name}.merge", FullName);

            await result1.EnsureSuccessExitCodeAsync();

            var result2 = await _process.ExecuteAsync(cancellationToken, "config", $"branch.{Name}.remote", remote.Name);

            await result2.EnsureSuccessExitCodeAsync();

            return new GitProcessBranch(_process, _repository, FullName, Name, new GitProcessRemote(_process, remote.Name));
        }
    }
}
