using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UpDock.CommandLine;
using UpDock.Files;

namespace UpDock
{
    public class ReportGenerator
    {
        private readonly CommandLineOptions _options;
        private readonly IFileProvider _provider;
        private readonly List<ReportEntry> _pullRequests = new();

        public ReportGenerator(CommandLineOptions options, IFileProvider provider)
        {
            _options = options;
            _provider = provider;
        }

        public void AddPullRequest(string url)
        {
            _pullRequests.Add(new ReportEntry(url));
        }

        public async Task GenerateReportAsync(CancellationToken cancellationToken)
        {
            if (_options.Report is null)
                return;

            var file = _provider.GetFile(_options.Report);

            if (file is null)
                return;

            using var stream = file.CreateWriteStream();

            await JsonSerializer.SerializeAsync(stream, _pullRequests, cancellationToken: cancellationToken);
        }

        private class ReportEntry
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }

            public ReportEntry(string url)
            {
                Url = url;
            }
        }
    }
}
