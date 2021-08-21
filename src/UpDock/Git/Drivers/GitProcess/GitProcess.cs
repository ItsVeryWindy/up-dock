using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UpDock.Files;

namespace UpDock.Git.Drivers
{
    public class GitProcess
    {
        private readonly IDirectoryInfo _directory;
        private readonly ILogger<GitProcess> _logger;

        public GitProcess(IDirectoryInfo directory, ILogger<GitProcess> logger)
        {
            _directory = directory;
            _logger = logger;
        }

        public Task<GitProcessResult> ExecuteAsync(CancellationToken cancellationToken, params string[] args)
        {
            var tcs = new TaskCompletionSource<GitProcessResult>();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo("git")
                {
                    UseShellExecute = false,
                    WorkingDirectory = _directory.AbsolutePath,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                },
                EnableRaisingEvents = true
            };

            foreach (var arg in args)
            {
                process.StartInfo.ArgumentList.Add(arg);
            }

            var ctr = cancellationToken.Register(() => process.Dispose());

            process.Exited += (sender, args) => {
                tcs.SetResult(new GitProcessResult(process, _logger));
                ctr.Dispose();
            };

            process.Disposed += (sender, args) => {
                ctr.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
    }
}
