using System;
using Octokit;

namespace UpDock
{
    internal class GitHubRepository : IRepository
    {
        private readonly Repository _repository;

        public GitHubRepository(Repository repository)
        {
            _repository = repository;
        }

        public string FullName => _repository.FullName;

        public string CloneUrl => _repository.CloneUrl;

        public DateTimeOffset? PushedAt => _repository.PushedAt;

        public string Name => _repository.Name;

        public string Owner => _repository.Owner.Login;

        public string DefaultBranch => _repository.DefaultBranch;
    }
}
