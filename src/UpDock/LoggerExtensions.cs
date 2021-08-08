using System;
using UpDock.Git;
using UpDock.Imaging;
using Microsoft.Extensions.Logging;

namespace UpDock
{
    public static class LoggerExtensions
    {
        private static readonly Action<ILogger, string, int, string, DockerImageTemplatePattern, DockerImageTemplate, Exception> _identifiedFromPattern = LoggerMessage.Define<string, int, string, DockerImageTemplatePattern, DockerImageTemplate>(LogLevel.Information, new EventId(0, "IdentifiedFromPattern"), "Identified '{CurrentVersion}' on line {LineNumber} of file {File}, from pattern {DockerImagePattern} with template {DockerImageTemplate}");
        private static readonly Action<ILogger, string, string, Exception> _replacingOutdatedVersion = LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(0, "ReplacingOutdatedVersion"), "Version is outdated, replacing '{CurrentVersion}' with '{LatestVersion}'");
        private static readonly Action<ILogger, string, Exception> _startedProcessingRepository = LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, "StartedProcessingRepository"), "Started processing repository {Repository}");
        private static readonly Action<ILogger, string, Exception> _skippedProcessingRepository = LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, "SkippedProcessingRepository"), "Skipped processing repository {Repository}");
        private static readonly Action<ILogger, string, Exception> _finishedProcessingRepository = LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, "FinishedProcessingRepository"), "Finished processing repository {Repository}");
        private static readonly Action<ILogger, string, Exception> _noChanges = LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, "NoChanges"), "No changes to be made in repository {Repository}");
        private static readonly Action<ILogger, string, Exception> _changesDetected = LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, "ChangesDetected"), "Changes detected in repository {Repository}");

        public static void LogIdentifiedFromPattern(this ILogger logger, string currentVersion, int lineNumber, IRepositoryFileInfo file, DockerImagePattern pattern) => _identifiedFromPattern(logger, currentVersion, lineNumber, file.RelativePath, pattern.Pattern, pattern.Image.Template, null!);
        public static void LogReplacingOutdatedVersion(this ILogger logger, string currentVersion, string latestVersion) => _replacingOutdatedVersion(logger, currentVersion, latestVersion, null!);
        public static void LogStartedProcessingRepository(this ILogger logger, IRemoteGitRepository repository) => _startedProcessingRepository(logger, repository.FullName, null!);
        public static void LogSkippedProcessingRepository(this ILogger logger, IRemoteGitRepository repository) => _skippedProcessingRepository(logger, repository.FullName, null!);
        public static void LogFinishedProcessingRepository(this ILogger logger, IRemoteGitRepository repository) => _finishedProcessingRepository(logger, repository.FullName, null!);
        public static void LogNoChangesInRepository(this ILogger logger, IRemoteGitRepository repository) => _noChanges(logger, repository.FullName, null!);
        public static void LogChangesDetectedInRepository(this ILogger logger, IRemoteGitRepository repository) => _changesDetected(logger, repository.FullName, null!);
    }
}
