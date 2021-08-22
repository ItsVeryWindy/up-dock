using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UpDock.CommandLine;
using UpDock.Files;
using UpDock.Git.Drivers;

namespace UpDock.Git
{
    public class LocalGitRepositoryFactory : ILocalGitRepositoryFactory
    {
        private readonly CommandLineOptions _options;
        private readonly IGitDriver _driver;
        private readonly IFileProvider _provider;
        private readonly ILogger<LocalGitRepository> _logger;

        public LocalGitRepositoryFactory(CommandLineOptions options, IGitDriver driver, IFileProvider provider, ILogger<LocalGitRepository> logger)
        {
            _provider = provider;
            _options = options;
            _driver = driver;
            _logger = logger;
        }

        public async Task<ILocalGitRepository> CreateAsync(string cloneUrl, string dir, IRemoteGitRepository remoteGitRepository, CancellationToken cancellationToken)
        {
            var directory = _provider.GetDirectory(dir);

            CleanupRepository(directory);

            var repository = await _driver.CloneAsync(cloneUrl, directory, _options.Token, cancellationToken);

            return new LocalGitRepository(repository, _options, remoteGitRepository, _logger);
        }

        private void CleanupRepository(IDirectoryInfo dir)
        {
            if (!dir.Exists)
                return;

            _logger.LogInformation("{Directory} already exists, cleaning up", dir);

            NormalizeAttributes(dir);

            dir.Delete();
        }

        private static void NormalizeAttributes(IDirectoryInfo directory)
        {
            foreach (var file in directory.Files)
            {
                file.SetAttributes(FileAttributes.Normal);
            }

            foreach (var subDirectory in directory.Directories)
            {
                NormalizeAttributes(subDirectory);
            }

            directory.SetAttributes(FileAttributes.Normal);
        }
    }
}
