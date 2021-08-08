using System;
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
        ILocalGitRepository CheckoutRepository();
        Task CreatePullRequestAsync(IRemoteGitRepository forkedRepository, PullRequest pullRequest);
    }
}
