using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UpDock.Files;

namespace UpDock.Git.Drivers
{
    public class GitProcessDriver : IGitDriver
    {
        private readonly IFileProvider _provider;
        private readonly ILogger<GitProcess> _logger;

        public GitProcessDriver(IFileProvider provider, ILogger<GitProcess> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public async Task<IRepository> CloneAsync(string cloneUrl, IDirectoryInfo directory, string? token, CancellationToken cancellationToken)
        {
            var builder = new UriBuilder(cloneUrl); ;

            if (token is not null)
            {
                builder.UserName = "username";
                builder.Password = token;
            }

            directory.Create();

            var process = new GitProcess(directory, _logger);

            var result = await process.ExecuteAsync(cancellationToken, "clone", "--depth" , "1", "--no-single-branch", builder.Uri.ToString(), ".");

            await result.EnsureSuccessExitCodeAsync();

            return new GitProcessRepository(process, directory);
        }

        public async Task CreateRemoteAsync(IDirectoryInfo remoteDirectory, CancellationToken cancellationToken)
        {
            var process = new GitProcess(remoteDirectory, _logger);

            using var result = await process.ExecuteAsync(cancellationToken, "init", "--bare", ".");

            await result.EnsureSuccessExitCodeAsync();
        }
    }
}
