﻿using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using UpDock.Imaging;
using NUnit.Framework;
using System;

namespace UpDock.Tests
{
    public class ConfigurationOptionsTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private ConfigurationOptions _options;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [SetUp]
        public void SetUp()
        {
            _options = new ConfigurationOptions();
        }

        [Test]
        public async Task ShouldParseConfigurationFile()
        {
            var ms = await CreateStream(new
            {
                include = "include",
                exclude = "exclude",
                templates = new object[]
                {
                    "example-image",
                    new
                    {
                        image = "example-repository.com/example-image",
                    },
                    new
                    {
                        pattern = "example-pattern",
                        image = "example-repository.com/example-image:example-tag",
                    }
                }
            });

            _options.Populate(ms);

            Assert.That(_options.Include, Has.Count.EqualTo(1));
            Assert.That(_options.Include, Does.Contain("include"));
            Assert.That(_options.Exclude, Has.Count.EqualTo(1));
            Assert.That(_options.Exclude, Does.Contain("exclude"));
            Assert.That(_options.Patterns, Has.Count.EqualTo(3));

            var first = _options.Patterns.First();

            Assert.That(first.ToString(), Is.EqualTo("example-image:{v*}"));
            Assert.That(first.Template.Image, Is.EqualTo("library/example-image"));
            Assert.That(first.Template.Repository, Is.EqualTo(DockerImageTemplate.DefaultRepository));
            Assert.That(first.Template.Tag, Is.EqualTo("{v*}"));

            var second = _options.Patterns.Skip(1).First();

            Assert.That(second.ToString(), Is.EqualTo("example-repository.com/example-image:{v*}"));
            Assert.That(second.Template.Image, Is.EqualTo("example-image"));
            Assert.That(second.Template.Repository.ToString(), Is.EqualTo("https://example-repository.com/"));
            Assert.That(second.Template.Tag, Is.EqualTo("{v*}"));

            var third = _options.Patterns.Skip(2).First();

            Assert.That(third.ToString(), Is.EqualTo("example-pattern"));
            Assert.That(third.Template.Image, Is.EqualTo("example-image"));
            Assert.That(third.Template.Repository?.ToString(), Is.EqualTo("https://example-repository.com/"));
            Assert.That(third.Template.Tag, Is.EqualTo("example-tag"));
        }

        [Test]
        public async Task ShouldHandleImageMissing()
        {
            var ms = await CreateStream(new {
                templates = new object[]
                {
                    new
                    {
                    }
                }
            });

            Assert.That(() => _options.Populate(ms), Throws.InvalidOperationException.And.Message.EqualTo("Invalid configuration file: expected image was not found"));
        }

        [Test]
        public async Task ShouldHandleImageNotBeingAString()
        {
            var ms = await CreateStream(new {
                templates = new object[]
                {
                    new
                    {
                        image = Array.Empty<object>()
                    }
                }
            });

            Assert.That(() => _options.Populate(ms), Throws.InvalidOperationException.And.Message.EqualTo("Invalid configuration file: expected image to be a string"));
        }

        [Test]
        public async Task ShouldHandleGroupNotBeingAString()
        {
            var ms = await CreateStream(new {
                templates = new object[]
                {
                    new
                    {
                        image = "example-image",
                        group = Array.Empty<object>()
                    }
                }
            });

            Assert.That(() => _options.Populate(ms), Throws.InvalidOperationException.And.Message.EqualTo("Invalid configuration file: expected group to be a string"));
        }

        [Test]
        public async Task ShouldHandlePatternNotBeingAString()
        {
            var ms = await CreateStream(new {
                templates = new object[]
                {
                    new
                    {
                        image = "example-image",
                        pattern = Array.Empty<object>()
                    }
                }
            });

            Assert.That(() => _options.Populate(ms), Throws.InvalidOperationException.And.Message.EqualTo("Invalid configuration file: expected pattern to be a string"));
        }

        [Test]
        public void ShouldCreateHash()
        {
            Assert.That(_options.CreateHash(), Is.EqualTo("8dc6e36ab1b53c503309e7e07ace0ba933cc11888166402a7993c9899995d6d0"));
        }

        [Test]
        public void ShouldCreateHashNotInfuencedBySomeOptions()
        {
            _options.Search = "hello";
            _options.Token = "token";
            _options.Authentication.Add("test", new AuthenticationOptions("username", "password"));

            Assert.That(_options.CreateHash(), Is.EqualTo("8dc6e36ab1b53c503309e7e07ace0ba933cc11888166402a7993c9899995d6d0"));
        }

        [TestCase("abcd", "abcd:{v}", null, "c411452192016238d5d2101fdc5ce2e2634ff829ab14e69c21fb07a9566d2f4a")]
        [TestCase("abcd", "abcd:{v}", "my-group", "b5a77f3efdb893fcfb6a50e57f1f4254fca97be5bba6dba9c617ab429922ff78")]
        [TestCase("abcd", "abcde:{v}", null, "cc46d921ab4192ee46c2d859da7243308ba36835bdd188a8a6a99d0b087eeb8e")]
        [TestCase("efgh", "efgh:{v}", null, "fa162998e47e794187a931f68ce6bc7048f653679b9fdb09151a013c3213c697")]
        [TestCase("abcd", "abcd:{v3.*}", null, "9bbc863024381b3ae4d5990fd2b0cd783af86b386648cf3b5212829c8dccb3cc")]
        public void ShouldCreateDifferentHashBasedOnTemplateAndPattern(string template, string pattern, string group, string expectedHash)
        {
            _options.Patterns.Add(DockerImageTemplate.Parse(template).CreatePattern(pattern, group));

            Assert.That(_options.CreateHash(), Is.EqualTo(expectedHash));
        }

        [Test]
        public void ShouldCreateDifferentHashBasedOnInclude()
        {
            _options.Include.Add("file/path");

            Assert.That(_options.CreateHash(), Is.EqualTo("fa473d06ec8465812e01c7b8ffeeb6a3b2bb77fb2b1a1833949b1c71f891b2f4"));
        }

        [Test]
        public void ShouldCreateDifferentHashBasedOnExclude()
        {
            _options.Exclude.Add("file/path");

            Assert.That(_options.CreateHash(), Is.EqualTo("c118d3e8c488dd95702ba119786c1ca91a33ac34c8c38c512d4aa782384c5533"));
        }

        [Test]
        public void ShouldCreateDifferentHashBasedOnDryRun()
        {
            _options.DryRun = true;

            Assert.That(_options.CreateHash(), Is.EqualTo("c6017bf39bacbd96c9116981c9ea5e6268272a2a086ec64faefc2d9912f12abd"));
        }

        [Test]
        public void ShouldCreateDifferentHashBasedOnAllowDowngrade()
        {
            _options.AllowDowngrade = true;

            Assert.That(_options.CreateHash(), Is.EqualTo("492764761973cd7e5886902ede96bcc5579acb2f202c5f13f7a146c992bc4056"));
        }

        private static async Task<MemoryStream> CreateStream(object obj)
        {
            var ms = new MemoryStream();

            await JsonSerializer.SerializeAsync(ms, obj);

            ms.Position = 0;

            return ms;
        }
    }
}
