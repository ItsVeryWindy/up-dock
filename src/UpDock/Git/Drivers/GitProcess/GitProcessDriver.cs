using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using UpDock.Files;

namespace UpDock.Git.Drivers
{
    public class GitProcessDriver : IGitDriver
    {
        private readonly ILogger<GitProcess> _logger;

        public GitProcessDriver(ILogger<GitProcess> logger)
        {
            _logger = logger;
        }

        public async Task<IRepository> CloneAsync(string cloneUrl, IDirectoryInfo directory, string? token, CancellationToken cancellationToken)
        {
            var builder = new UriBuilder(cloneUrl);

            if (token is not null)
            {
                builder.UserName = "username";
                builder.Password = token;
            }

            directory.Create();

            var process = new GitProcess(directory, _logger);

            await EnsureValidVersionAsync(process, cancellationToken);

            var result = await process.ExecuteAsync(cancellationToken, "clone", "--depth" , "1", "--no-single-branch", builder.Uri.ToString(), ".");

            await result.EnsureSuccessExitCodeAsync();

            return new GitProcessRepository(process, directory);
        }

        public async Task<string> CreateRemoteAsync(IDirectoryInfo remoteDirectory, CancellationToken cancellationToken)
        {
            var process = new GitProcess(remoteDirectory, _logger);

            await EnsureValidVersionAsync(process, cancellationToken);

            using var result = await process.ExecuteAsync(cancellationToken, "init", "--bare", ".");

            await result.EnsureSuccessExitCodeAsync();

            var cloneUrl = new Uri(new Uri("file://"), remoteDirectory.AbsolutePath).ToString();

            _logger.LogInformation("Creating remote {Remote}", cloneUrl);

            return cloneUrl;
        }

        private static readonly FloatRange MinVersion = new(NuGetVersionFloatBehavior.AbsoluteLatest, NuGetVersion.Parse("2.22"));

        private static async Task EnsureValidVersionAsync(GitProcess process, CancellationToken cancellationToken)
        {
            using var result = await process.ExecuteAsync(cancellationToken, "--version");

            await result.EnsureSuccessExitCodeAsync();

            var versionStr = await result.ReadContentAsync();

            const string preText = "git version ";

            if (versionStr.StartsWith(preText))
            {
                versionStr = versionStr.Substring(preText.Length);

                var index = versionStr.IndexOf('.');

                if(index >= 0)
                {
                    index = versionStr.IndexOf('.', ++index);

                    if(index >= 0)
                    {
                        index = versionStr.IndexOf('.', ++index);

                        if(index >= 0)
                        {
                            versionStr = versionStr.Substring(0, index);
                        }
                    }
                }
            }

            var version = NuGetVersion.Parse(versionStr);

            if (MinVersion.Satisfies(version))
                return;

            await result.CauseFailureAsync("Unsupported version");
        }
    }
}
