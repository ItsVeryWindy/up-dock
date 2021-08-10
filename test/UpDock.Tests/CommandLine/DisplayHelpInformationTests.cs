using UpDock.CommandLine;
using NUnit.Framework;

namespace UpDock.Tests.CommandLine
{
    public class DisplayHelpInformationTests
    {
        private StubConsoleWriter _writer = null!;
        private DisplayHelpInformation _displayHelpInformation = null!;

        [SetUp]
        public void SetUp()
        {
            _writer = new StubConsoleWriter();

            _displayHelpInformation = new DisplayHelpInformation(_writer, new StubProcessInfo());
        }

        [Test]
        public void ShouldPrintHelpInformation()
        {
            _displayHelpInformation.Display<CommandLineOptions>();

            Assert.That(_writer.Lines, Is.EquivalentTo(new string?[] {
                "Usage: ProcessName [OPTIONS]",
                null,
                "Automatically update docker images in github repositories.",
                null,
                "Options:",
                "--allow-downgrade/-l    Allow downgrading if the version is higher than the one specified",
                "--auth/-a               Authentication for a repository",
                "--cache                 Cache the results from this run to re-use in another",
                "--config/-c             Default configuration to apply",
                "--dry-run/-d            Run without creating pull requests",
                "--email/-e*             Email to use in the commit",
                "--help/-h               Display help information",
                "--report/-r             Output a report to a file on the pull requests that were created",
                "--search/-s*            Search query to get repositories",
                "--template/-i           A template to apply",
                "--token/-t              GitHub token to access the repository",                
                "--version/-v            Display what the version is",
                null,
                "Prefixing the argument with an @ (eg. -@a, --@argument) will signify that value",
                "should come from a line in standard input. Multiple arguments may be specified",
                "this way and will be processed in the order that they appear."
            }));
        }
    }
}
