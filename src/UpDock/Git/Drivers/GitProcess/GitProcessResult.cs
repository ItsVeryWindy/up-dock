using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace UpDock.Git.Drivers
{
    public struct GitProcessResult : IDisposable
    {
        private readonly Process _process;
        private readonly ILogger _logger;

        internal GitProcessResult(Process process, ILogger logger)
        {
            _process = process;
            _logger = logger;
        }

        public async Task<string> ReadContentAsync() => (await _process.StandardOutput.ReadToEndAsync()).Trim();

        public Task EnsureSuccessExitCodeAsync() => EnsureExitCodeAsync(0);

        public Task EnsureExitCodeAsync(int exitCode)
        {
            if (_process.ExitCode == exitCode)
                return Task.CompletedTask;

            return HandleFailureAsync();
        }

        public async Task<int> EnsureExitCodeAsync(int exitCode1, int exitCode2)
        {
            if (_process.ExitCode == exitCode1)
                return exitCode1;

            if (_process.ExitCode == exitCode2)
                return exitCode2;

            await HandleFailureAsync();

            return 1;
        }

        public Task CauseFailureAsync(string message)
        {
            _logger.LogError("Git command failure: {Message}", message);

            return HandleFailureAsync();
        }

        private async Task HandleFailureAsync()
        {
            var outStr = await _process.StandardOutput.ReadToEndAsync();
            var errorStr = await _process.StandardError.ReadToEndAsync();

            _logger.LogError("Git command failure exit code: {ExitCode}", _process.ExitCode);
            _logger.LogError("Git command failure standard output: {Output}", outStr);
            _logger.LogError("Git command failure standard error: {Output}", errorStr);

            throw new GitProcessException(_process);
        }

        public void Dispose() => _process.Dispose();
        public async IAsyncEnumerable<string> ReadLinesAsync()
        {
            string? line;

            while ((line = await _process.StandardOutput.ReadLineAsync()) is not null)
            {
                yield return line;
            }
        }
    }
}
