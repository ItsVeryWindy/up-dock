using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using DockerUpgradeTool.CommandLine;
using NUnit.Framework;

namespace DockerUpgradeTool.Tests.CommandLine
{
    public class CommandLineParserTests
    {
        private const string Argument = "--argument";

        #pragma warning disable CS8618
        private CommandLineParser _parser;
        #pragma warning restore CS8618

        [SetUp]
        public void SetUp() => _parser = new CommandLineParser();

        [Test]
        public void ShouldParseBooleans()
        {
            var arguments = _parser.Parse<Options<bool>>(new[] { Argument });

            Assert.That(arguments, Has.Count.EqualTo(1));

            var argument = arguments.First();

            Assert.That(argument.Argument, Is.EqualTo(Argument));
            Assert.That(argument.Index, Is.EqualTo(0));
            Assert.That(argument.OriginalValue, Is.Null);
            Assert.That(argument.Value, Is.True);
        }

        [Test]
        public void ShouldHandlePropertiesThatDontExist()
        {
            var arguments = _parser.Parse<Options<bool>>(new[] { "--exist" });

            Assert.That(arguments, Has.Count.EqualTo(1));

            var argument = arguments.First();

            Assert.That(argument.Argument, Is.EqualTo("--exist"));
            Assert.That(argument.Index, Is.EqualTo(0));
            Assert.That(argument.OriginalValue, Is.Null);
            Assert.That(argument.Value, Is.Null);
            Assert.That(argument.Property, Is.Null);
            Assert.That(argument.Errors, Has.Count.EqualTo(1));
            Assert.That(argument.Errors.First(), Is.EqualTo("Argument does not exist"));
        }

        [Test]
        public void ShouldParseStrings()
        {
            const string value = "value";

            var arguments = _parser.Parse<Options<string>>(new[] { Argument, value });

            Assert.That(arguments, Has.Count.EqualTo(1));

            var argument = arguments.First();

            Assert.That(argument.Argument, Is.EqualTo(Argument));
            Assert.That(argument.Index, Is.EqualTo(0));
            Assert.That(argument.OriginalValue, Is.EqualTo(value));
            Assert.That(argument.Value, Is.EqualTo(value));
        }

        [Test]
        public void ShouldConvertValues()
        {
            const string value = "123";

            var arguments = _parser.Parse<Options<int>>(new[] { Argument, value });

            Assert.That(arguments, Has.Count.EqualTo(1));

            var argument = arguments.First();

            Assert.That(argument.Argument, Is.EqualTo(Argument));
            Assert.That(argument.Index, Is.EqualTo(0));
            Assert.That(argument.OriginalValue, Is.EqualTo(value));
            Assert.That(argument.Value, Is.EqualTo(123));
        }

        [Test]
        public void ShouldHandleBadConversions()
        {
            const string value = "value";

            var arguments = _parser.Parse<Options<int>>(new[] { Argument, value });

            Assert.That(arguments, Has.Count.EqualTo(1));

            var argument = arguments.First();

            Assert.That(argument.Argument, Is.EqualTo(Argument));
            Assert.That(argument.Index, Is.EqualTo(0));
            Assert.That(argument.OriginalValue, Is.EqualTo(value));
            Assert.That(argument.Value, Is.Null);
            Assert.That(argument.Errors, Has.Count.EqualTo(1));
            Assert.That(argument.Errors.First(), Is.EqualTo("Input string was not in a correct format."));
        }

        [Test]
        public void ShouldHandleMissingValues()
        {
            var arguments = _parser.Parse<Options<string>>(new[] { Argument });

            Assert.That(arguments, Has.Count.EqualTo(1));

            var argument = arguments.First();

            Assert.That(argument.Argument, Is.EqualTo(Argument));
            Assert.That(argument.Index, Is.EqualTo(0));
            Assert.That(argument.OriginalValue, Is.Null);
            Assert.That(argument.Value, Is.Null);
            Assert.That(argument.Errors, Has.Count.EqualTo(1));
            Assert.That(argument.Errors.First(), Is.EqualTo("Value not specified"));
        }

        [Test]
        public void ShouldParseAnArrayOfValues()
        {
            const string value = "value";

            var arguments = _parser.Parse<Options<string[]>>(new[] { Argument, value, Argument, value });

            Assert.That(arguments, Has.Count.EqualTo(2));

            var firstArgument = arguments.First();

            Assert.That(firstArgument.Argument, Is.EqualTo(Argument));
            Assert.That(firstArgument.Index, Is.EqualTo(0));
            Assert.That(firstArgument.OriginalValue, Is.EqualTo(value));
            Assert.That(firstArgument.Value, Is.EqualTo(value));

            var lastArgument = arguments.Last();

            Assert.That(lastArgument.Argument, Is.EqualTo(Argument));
            Assert.That(lastArgument.Index, Is.EqualTo(1));
            Assert.That(lastArgument.OriginalValue, Is.EqualTo(value));
            Assert.That(lastArgument.Value, Is.EqualTo(value));
        }

        [Test]
        public void ShouldParseAListOfValues()
        {
            const string value = "value";

            var arguments = _parser.Parse<Options<List<string>>>(new[] { Argument, value, Argument, value });

            Assert.That(arguments, Has.Count.EqualTo(2));

            var firstArgument = arguments.First();

            Assert.That(firstArgument.Argument, Is.EqualTo(Argument));
            Assert.That(firstArgument.Index, Is.EqualTo(0));
            Assert.That(firstArgument.OriginalValue, Is.EqualTo(value));
            Assert.That(firstArgument.Value, Is.EqualTo(value));

            var lastArgument = arguments.Last();

            Assert.That(lastArgument.Argument, Is.EqualTo(Argument));
            Assert.That(lastArgument.Index, Is.EqualTo(1));
            Assert.That(lastArgument.OriginalValue, Is.EqualTo(value));
            Assert.That(lastArgument.Value, Is.EqualTo(value));
        }

        [Test]
        public void ShouldConvertValuesWithTypeConverter()
        {
            const string value = "value";

            var arguments = _parser.Parse<OptionsWithTypeConverter>(new[] { Argument, value });

            Assert.That(arguments, Has.Count.EqualTo(1));

            var argument = arguments.First();

            Assert.That(argument.Argument, Is.EqualTo(Argument));
            Assert.That(argument.Index, Is.EqualTo(0));
            Assert.That(argument.OriginalValue, Is.EqualTo(value));
            Assert.That(argument.Value, Is.EqualTo("PREFIXvalue"));
        }

        [Test]
        public void ShouldHandlePropertiesThatAreRequired()
        {
            var arguments = _parser.Parse<OptionsWithRequiredProperty>(new string[0]);

            Assert.That(arguments, Has.Count.EqualTo(1));

            var argument = arguments.First();

            Assert.That(argument.Argument, Is.EqualTo("--argument"));
            Assert.That(argument.Index, Is.EqualTo(0));
            Assert.That(argument.OriginalValue, Is.Null);
            Assert.That(argument.Value, Is.Null);
        }

        public class Options<T>
        {
            [Shortcut("--argument")]
#pragma warning disable CS8618
            public T Property { get; set; }
#pragma warning disable CS8618

        }

        public class OptionsWithRequiredProperty
        {
            [Shortcut("--argument")]
            [Required]
            public string? Property { get; set; }
        }

        public class OptionsWithTypeConverter
        {
            [Shortcut("--argument")]
            [TypeConverter(typeof(StringTypeConverter))]
            public string? Property { get; set; }

            private class StringTypeConverter : TypeConverter
            {
                public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string);

                public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) => $"PREFIX{value}";
            }
        }
    }
}
