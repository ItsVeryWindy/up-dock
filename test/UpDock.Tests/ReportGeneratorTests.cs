using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using UpDock.CommandLine;
using UpDock.Files;

namespace UpDock.Tests
{
    public class ReportGeneratorTests
    {
        private ReportGenerator _reportGenerator = null!;
        private StubFileProvider _provider = null!;
        private CommandLineOptions _options = null!;

        [SetUp]
        public void SetUp()
        {
            _provider = new StubFileProvider();
            _options = new CommandLineOptions
            {
                Report = "/made/up/path"
            };

            var sp = TestUtilities
                .CreateServices()
                .AddSingleton(_options)
                .AddSingleton<IFileProvider>(_provider)
                .BuildServiceProvider();

            _reportGenerator = sp.GetRequiredService<ReportGenerator>();
        }

        [Test]
        public async Task ShouldNotSaveReportIfNotSpecified()
        {
            _options.Report = null;

            await _reportGenerator.GenerateReportAsync(CancellationToken.None);
        }

        [Test]
        public async Task ShouldSaveReport()
        {
            _reportGenerator.AddPullRequest("https://my-pull-request");

            await _reportGenerator.GenerateReportAsync(CancellationToken.None);

            var newContents = await TestUtilities.GetStringAsync(_provider.GetFile(_options.Report!).CreateReadStream());

            Assert.That(newContents, Is.EqualTo("[{\"url\":\"https://my-pull-request\"}]"));
        }
    }
}
