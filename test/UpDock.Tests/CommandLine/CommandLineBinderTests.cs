using System.Collections.Generic;
using UpDock.CommandLine;
using NUnit.Framework;

namespace UpDock.Tests.CommandLine
{
    public class CommandLineBinderTests
    {
        private CommandLineBinder _binder = null!;

        [SetUp]
        public void SetUp() => _binder = new CommandLineBinder();

        [Test]
        public void ShouldNotBindIfArgumentNotSpecified()
        {
            var options = new Options<object>();

            _binder.Bind(new List<CommandLineArgument>(), options);

            Assert.That(options.Property, Is.Null);
        }

        [Test]
        public void ShouldBindIfArgumentSpecified()
        {
            var options = new Options<object>();

            var value = new object();

            _binder.Bind(new List<CommandLineArgument>()
            {
                new CommandLineArgument("--argument", null, value, options.GetType().GetProperty(nameof(Options<object>.Property))!, 0)
            }, options);

            Assert.That(options.Property, Is.EqualTo(value));
        }

        [Test]
        public void ShouldBindAnArray()
        {
            var options = new Options<object[]>();

            var value = new object();

            _binder.Bind(new List<CommandLineArgument>()
            {
                new CommandLineArgument("--argument", null, value, options.GetType().GetProperty(nameof(Options<object[]>.Property))!, 0),
                new CommandLineArgument("--argument", null, value, options.GetType().GetProperty(nameof(Options<object[]>.Property))!, 0)
            }, options);

            Assert.That(options.Property, Is.EquivalentTo(new[] { value, value }));
        }

        public class Options<T> where T : class
        {
            public T? Property { get; set; }
        }
    }
}
