using System;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Git;

namespace UpDock.Tests.Stubs
{
    public class StubRemoteGitRepository : IRemoteGitRepository
    {
        public string FullName => "FullName";

        public string CloneUrl => "CloneUrl";

        public DateTimeOffset? PushedAt => DateTimeOffset.MinValue;

        public string Name => "Name";

        public string Owner => "Owner";

        public string DefaultBranch => "DefaultBranch";

        public Task<ILocalGitRepository> CheckoutRepositoryAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<(string url, string title)?> CreatePullRequestAsync(IRemoteGitRepository forkedRepository, PullRequest newPullRequest) => throw new NotImplementedException();

        public Task<IRemoteGitRepository> ForkRepositoryAsync() => throw new NotImplementedException();
    }
}
