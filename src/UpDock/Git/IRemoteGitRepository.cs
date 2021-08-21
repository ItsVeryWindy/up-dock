using System;
using System.Threading;
using System.Threading.Tasks;

namespace UpDock.Git
{
    public interface IRemoteGitRepository
    {
        string CloneUrl { get; }
        string Owner { get; }
        string Name { get; }
        string DefaultBranch { get; }
        DateTimeOffset? PushedAt { get; }
        string FullName { get; }

        Task<IRemoteGitRepository> ForkRepositoryAsync();
        Task<ILocalGitRepository> CheckoutRepositoryAsync(CancellationToken cancellationToken);
        Task<(string url, string title)?> CreatePullRequestAsync(IRemoteGitRepository forkedRepository, PullRequest pullRequest);
    }
}
