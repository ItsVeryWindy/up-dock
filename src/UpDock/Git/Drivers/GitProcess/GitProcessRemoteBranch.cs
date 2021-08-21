using System.Threading;
using System.Threading.Tasks;

namespace UpDock.Git.Drivers
{
    public class GitProcessRemoteBranch : IRemoteReference
    {
        private readonly GitProcess _process;
        private readonly GitProcessRemote _remote;

        public string FullName { get; }

        public GitProcessRemoteBranch(GitProcess process, GitProcessRemote remote, string fullName)
        {
            _process = process;
            _remote = remote;
            FullName = fullName;
        }

        public async Task RemoveAsync(CancellationToken cancellationToken)
        {
            using var result = await _process.ExecuteAsync(cancellationToken, "push", _remote.Name, $"+:{FullName}");

            await result.EnsureSuccessExitCodeAsync();
        }
    }
}
