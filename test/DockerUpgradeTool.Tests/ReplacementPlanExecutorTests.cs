using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            var stream = typeof(ReplacementPlanExecutorTests).Assembly.GetManifestResourceStream("DockerUpgradeTool.Tests.Files.Dockerfile")!;

            var provider = new PhysicalFileProvider();

            var tempFile = provider.CreateTemporaryFile();

            await using (var s = tempFile.CreateWriteStream())
            {
                await stream.CopyToAsync(s);
            }

            var sp = Program.CreateServices(new CommandLineOptions(), CancellationToken.None)
                .BuildServiceProvider();

            var executor = sp.GetRequiredService<IReplacementPlanExecutor>();

            var replacements = new List<TextReplacement>
            {
                new TextReplacement("group", tempFile, "mcr.microsoft.com/dotnet/core/sdk:3.1.101-alpine3.10", null!, "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11", null!, 0, 5)
            };

            await executor.ExecutePlanAsync(replacements, CancellationToken.None);

            var replacedFile = await File.ReadAllTextAsync(tempFile.Path);

            var expectedStream = typeof(ReplacementPlanExecutorTests).Assembly.GetManifestResourceStream("DockerUpgradeTool.Tests.Files.Dockerfile_expected")!;

            var reader = new StreamReader(expectedStream);

            var text = await reader.ReadToEndAsync();

            Assert.That(replacedFile, Is.EqualTo(text));
        }
    }
}
