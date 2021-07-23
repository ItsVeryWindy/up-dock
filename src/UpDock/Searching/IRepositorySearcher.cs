using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Octokit;

namespace UpDock
{
    public interface IRepositorySearcher
    {
        Task<IEnumerable<IRepository>> SearchAsync(string search, CancellationToken cancellationToken);
    }

    public class GitHubRepositorySearcher : IRepositorySearcher
    {
        private readonly IGitHubClient _client;

        public GitHubRepositorySearcher(IGitHubClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<IRepository>> SearchAsync(string search, CancellationToken cancellationToken)
        {
            var result =  await _client.Search.SearchRepo(new SearchRepositoriesRequest(search));

            return result.Items.Select(x => new GitHubRepository(x));
        }
    }
}
