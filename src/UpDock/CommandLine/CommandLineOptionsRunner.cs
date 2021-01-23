using System.Threading;
using System.Threading.Tasks;

namespace UpDock.CommandLine
{
    public class CommandLineOptionsRunner : ICommandLineRunner<CommandLineOptions>
    {
        private readonly IDisplayHelpInformation _displayHelpInformation;
        private readonly IProcessInfo _processInfo;
        private readonly IConsoleWriter _writer;
        private readonly IGitRepositoryProcessor _processor;

        public CommandLineOptionsRunner(IDisplayHelpInformation displayHelpInformation, IProcessInfo processInfo, IConsoleWriter writer, IGitRepositoryProcessor processor)
        {
            _displayHelpInformation = displayHelpInformation;
            _processInfo = processInfo;
            _writer = writer;
            _processor = processor;
        }

        public Task RunAsync(CommandLineOptions options, CancellationToken cancellationToken)
        {
            if (options.Help)
            {
                _displayHelpInformation.Display<CommandLineOptions>();

                return Task.CompletedTask;
            }

            if (options.Version)
            {
                _writer.WriteLine(_processInfo.Version);

                return Task.CompletedTask;
            }

            return _processor.ProcessAsync(cancellationToken);
        }
    }
}
