using System.IO;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using UpDock.CommandLine;
using UpDock.Files;

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

        public ILocalGitRepository Create(string cloneUrl, string dir, IRemoteGitRepository remoteGitRepository)
        {
            CleanupRepository(dir);

            var repository = _driver.Clone(cloneUrl, dir, _options.Token);

            return new LocalGitRepository(repository, _options, remoteGitRepository, _provider, _logger);
        }

        private void CleanupRepository(string dir)
        {
            if (!Directory.Exists(dir))
                return;

            _logger.LogInformation("{Directory} already exists, cleaning up", dir);

            NormalizeAttributes(dir);

            Directory.Delete(dir, true);
        }

        private static void NormalizeAttributes(string directoryPath)
        {
            var filePaths = Directory.GetFiles(directoryPath);
            var subDirectoryPaths = Directory.GetDirectories(directoryPath);

            foreach (var filePath in filePaths)
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
            }

            foreach (var subDirectoryPath in subDirectoryPaths)
            {
                NormalizeAttributes(subDirectoryPath);
            }

            File.SetAttributes(directoryPath, FileAttributes.Normal);
        }
    }
}
