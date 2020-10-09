using System.Threading;
using System.Threading.Tasks;

namespace DockerUpgradeTool.Tests.CommandLine
{
    internal class StubGitRepositoryProcessor : IGitRepositoryProcessor
    {
        public bool WasCalled { get; private set; }

        public Task ProcessAsync(CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }
}
