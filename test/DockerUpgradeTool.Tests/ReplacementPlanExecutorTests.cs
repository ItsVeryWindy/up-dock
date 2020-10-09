using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DockerUpgradeTool.CommandLine;
using DockerUpgradeTool.Files;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace DockerUpgradeTool.Tests
{
    public class ReplacementPlanExecutorTests
    {
        [Test]
        public async Task ShouldPerformReplaceOnSingleFile()
        {
            var stream = TestUtilities.GetResource("Files.Dockerfile")!;

            var provider = new StubFileProvider();

            var tempFile = provider.GetFile("Dockerfile");

            await stream.CopyToAsync(tempFile.CreateWriteStream());

            var sp = TestUtilities
                .CreateServices()
                .AddSingleton<IFileProvider>(provider)
                .BuildServiceProvider();

            var executor = sp.GetRequiredService<IReplacementPlanExecutor>();

            var replacements = new List<TextReplacement>
            {
                new TextReplacement("group", tempFile, "mcr.microsoft.com/dotnet/core/sdk:3.1.101-alpine3.10", null!, "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11", null!, 0, 5)
            };

            await executor.ExecutePlanAsync(replacements, CancellationToken.None);

            var replacedFile = await tempFile.CreateReadStream().GetStringAsync();

            var expectedFile = await TestUtilities.GetResource("Files.Dockerfile_expected").GetStringAsync();

            Assert.That(replacedFile, Is.EqualTo(expectedFile));
        }
    }
}
