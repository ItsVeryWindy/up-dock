using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DockerUpgrader.Files;

namespace DockerUpgrader
{
    public class ReplacementPlanExecutor : IReplacementPlanExecutor
    {
        private readonly IFileProvider _provider;

        public ReplacementPlanExecutor(IFileProvider provider)
        {
            _provider = provider;
        }

        public async Task ExecutePlanAsync(IReadOnlyCollection<TextReplacement> replacements, CancellationToken cancellationToken)
        {
            if (replacements.Count == 0)
                return;

            foreach (var replacementGroup in replacements.GroupBy(x => x.File))
            {
                var list = replacementGroup.ToList();

                var tempFile = await CreateTemporaryFileAsync(replacementGroup.Key, list, cancellationToken);

                replacementGroup.Key.Delete();

                tempFile.Move(replacementGroup.Key);
            }
        }

        private async Task<IFileInfo> CreateTemporaryFileAsync(IFileInfo file, IReadOnlyCollection<TextReplacement> replacements, CancellationToken cancellationToken)
        {
            var tempFile = _provider.CreateTemporaryFile();

            await using var inputFileStream = file.CreateReadStream();
            await using var outputFileStream = tempFile.CreateWriteStream();

            await using var sw = new StreamWriter(outputFileStream);
            using var sr = new StreamReader(inputFileStream);

            string? line;
            var lineNumber = 0;

            while ((line = await sr.ReadLineAsync()) != null)
            {
                var lineReplacements = replacements
                    .Where(x => x.LineNumber == lineNumber)
                    .ToList();

                if (lineReplacements.Count > 0)
                {
                    lineReplacements.Reverse();

                    foreach (var lineReplacement in lineReplacements)
                    {
                        line = line
                            .Remove(lineReplacement.Start, lineReplacement.From.Length)
                            .Insert(lineReplacement.Start, lineReplacement.To);
                    }
                }

                await sw.WriteLineAsync(line);

                lineNumber++;
            }

            return tempFile;
        }
    }
}
