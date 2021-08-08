using System.Collections.Generic;
using System.Linq;
using UpDock.CommandLine;
using NUnit.Framework;

namespace UpDock.Tests.CommandLine
{
    public class DisplayErrorMessageTests
    {
        private StubConsoleWriter _writer = null!;
        private DisplayErrorMessages _displayErrorMessages = null!;

        [SetUp]
        public void SetUp()
        {
            _writer = new StubConsoleWriter();

            _displayErrorMessages = new DisplayErrorMessages(_writer);
        }

        [Test]
        public void ShouldPrintErrorsToTheConsole()
        {
            _displayErrorMessages.Display(new List<CommandLineArgument>()
            {
                new CommandLineArgument("--argument", "value", null, null, 0)
                {
                    Errors =
                    {
                        "This is wrong"
                    }
                }
            });

            Assert.That(_writer.Lines, Is.EquivalentTo(new string?[] {
                "--argument: (value)",
                "            This is wrong",
                null
            }));
        }

        [Test]
        public void ShouldNotPrintNoErrorsToTheConsole()
        {
            _displayErrorMessages.Display(new List<CommandLineArgument>()
            {
                new CommandLineArgument("--argument", null, null, null, 0)
            });

            Assert.That(_writer.Lines, Is.EquivalentTo(new string?[] { null }));
        }
    }
}
