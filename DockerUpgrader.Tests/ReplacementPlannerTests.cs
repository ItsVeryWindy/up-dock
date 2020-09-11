using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DockerUpgrader.Imaging;
using DockerUpgrader.Nodes;
using DockerUpgrader.Registry;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace DockerUpgrader.Tests
{
    public class ReplacementPlannerTests
    {
        [TestCaseSource(nameof(TestCases))]
        public async Task ShouldReturnLinesToBeReplaced(DockerImageTemplatePattern pattern, string fileName, string expectedFrom, int expectedLineNumber, int expectedStart, string expectedTo)
        {
            var sp = Program.CreateServices(new CommandLineOptions(), CancellationToken.None)
                .AddSingleton<HttpMessageHandler>(new StaticResponseHandler())
                .BuildServiceProvider();

            await sp.GetRequiredService<IVersionCache>().UpdateCacheAsync(Enumerable.Repeat(pattern, 1), CancellationToken.None);

            var node = new SearchNodeBuilder().Add(pattern).Build();

            var planner = sp.GetRequiredService<IReplacementPlanner>();

            var stream = typeof(ReplacementPlannerTests).Assembly.GetManifestResourceStream($"DockerUpgrader.Tests.Files.{fileName}")!;

            var fileInfo = new StreamFileInfo(stream);

            var results = await planner.GetReplacementPlanAsync(fileInfo, node, CancellationToken.None);

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results.First().From, Is.EqualTo(expectedFrom));
            Assert.That(results.First().LineNumber, Is.EqualTo(expectedLineNumber));
            Assert.That(results.First().Start, Is.EqualTo(expectedStart));
            Assert.That(results.First().To, Is.EqualTo(expectedTo));
        }

        public static IEnumerable<TestCaseData> TestCases
        {
            get
            {
                yield return new TestCaseData(
                    DockerImageTemplate.ParseTemplate("mcr.microsoft.com/dotnet/core/sdk:{v}-alpine{v}").CreatePattern(true, true),
                    "Dockerfile", "mcr.microsoft.com/dotnet/core/sdk:3.1.101-alpine3.10", 0, 5, "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11");
                yield return new TestCaseData(
                    DockerImageTemplate.ParseTemplate("mcr.microsoft.com/dotnet/core/sdk:{v}-alpine{v}").CreatePattern("sdk:{v}-alpine{v}"),
                    "another_file.txt", "sdk:3.1.101-alpine3.10", 8, 20, "sdk:3.1.102-alpine3.11");
            }
        }
    }
}
