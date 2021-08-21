using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using UpDock.Files;

namespace UpDock.Git.Drivers
{
    public class LibGit2SharpDriver : IGitDriver
    {
        private readonly IFileProvider _provider;

        public LibGit2SharpDriver(IFileProvider provider)
        {
            _provider = provider;
        }

        public Task<IRepository> CloneAsync(string cloneUrl, IDirectoryInfo directory, string? token, CancellationToken cancellationToken)
        {
            UsernamePasswordCredentials CreateCredentials(string url, string user, SupportedCredentialTypes cred)
            {
                return new UsernamePasswordCredentials
                {
                    Username = "username", Password = token
                };
            }

            var co = new CloneOptions
            {
                CredentialsProvider = CreateCredentials
            };

            var path = Repository.Clone(cloneUrl, directory.AbsolutePath, co);

            return Task.FromResult<IRepository>(new LibGit2SharpRepository(new Repository(path), _provider, CreateCredentials));
        }

        public Task CreateRemoteAsync(IDirectoryInfo remoteDirectory, CancellationToken cancellationToken)
        {
            Repository.Init(remoteDirectory.AbsolutePath, true);

            return Task.CompletedTask;
        }
    }
}
