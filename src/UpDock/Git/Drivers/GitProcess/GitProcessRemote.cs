using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UpDock.Git.Drivers
{
    public class GitProcessRemote : IRemote
    {
        private readonly GitProcess _process;

        public string Name { get; }

        public GitProcessRemote(GitProcess process, string name)
        {
            _process = process;
            Name = name;
        }

        public async Task<IEnumerable<IRemoteReference>> GetReferencesAsync(CancellationToken cancellationToken)
        {
            using var result = await _process.ExecuteAsync(cancellationToken, "ls-remote", "--heads", "--refs", Name);

            await result.EnsureSuccessExitCodeAsync();

            var references = new List<IRemoteReference>();

            await foreach (var line in result.ReadLinesAsync())
            {
                var split = line.Split('\t')[1];

                references.Add(new GitProcessRemoteBranch(_process, this, split));
            }

            return references;
        }
    }
}
