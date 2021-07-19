using System;
using UpDock.Imaging;
using NUnit.Framework;

namespace UpDock.Tests
{
    public class DockerImageTemplateTests
    {
        [TestCase("nginx", "https://registry-1.docker.io/", "library/nginx", "nginx:{v*}")]
        [TestCase("library/nginx", "https://registry-1.docker.io/", "library/nginx", "library/nginx:{v*}")]
        [TestCase("repository.com/nginx", "https://repository.com/", "nginx", "repository.com/nginx:{v*}")]
        [TestCase("nginx:_{v}", "https://registry-1.docker.io/", "library/nginx", "nginx:_{v*}")]
        public void ShouldHandleValidString(string str, string expectedRepository, string expectedImage, string expectedPattern)
        {
            var template = DockerImageTemplate.Parse(str);

            Assert.That(template.Repository.ToString(), Is.EqualTo(expectedRepository));
            Assert.That(template.Image, Is.EqualTo(expectedImage));

            var pattern = template.CreatePattern(true, true, true);

            Assert.That(pattern.ToString(), Is.EqualTo(expectedPattern));
        }

        [TestCase("repositor_y.com/nginx", "The registry name for a template should not contain underscores.")]
        [TestCase("repository.com:abcd/nginx", "The registry name for the template is invalid.")]
        [TestCase("NGINX", "The image name for a template should only contain lowercase letters, digits, periods, underscores, or dashes.")]
        [TestCase("-nginx", "The image name for a template should not begin with a period, an underscore or a dash.")]
        [TestCase(".nginx", "The image name for a template should not begin with a period, an underscore or a dash.")]
        [TestCase("_nginx", "The image name for a template should not begin with a period, an underscore or a dash.")]
        [TestCase("nginx-", "The image name for a template should not end with a period, an underscore or a dash.")]
        [TestCase("nginx.", "The image name for a template should not end with a period, an underscore or a dash.")]
        [TestCase("nginx_", "The image name for a template should not end with a period, an underscore or a dash.")]
        [TestCase("nginx:{v", "The image tag for a template should have matching curly brackets.")]
        [TestCase("nginx:{vx1-2}", "The image tag for the template contains an invalid version range.")]
        [TestCase("nginx:tag:tag", "The image name for a template should only have one colon.")]
        [TestCase("nginx:-tag", "The image tag for a template cannot start with a period or a dash.")]
        [TestCase("nginx:.tag", "The image tag for a template cannot start with a period or a dash.")]
        public void ShouldHandleInvalidString(string str, string expectedError) => Assert.That(() => DockerImageTemplate.Parse(str), Throws.TypeOf<FormatException>().With.Message.EqualTo(expectedError));

        [Test]
        public void ShouldCreateDefaultGroupForPatternWhenGroupNotSpecified()
        {
            var pattern = DockerImageTemplate.Parse("nginx").CreatePattern("{v}");

            Assert.That(pattern.Group, Is.EqualTo("registry-1.docker.io/library/nginx:{v}"));
        }

        [Test]
        public void ShouldCreateDefaultGroupWhenGroupNotSpecified()
        {
            var pattern = DockerImageTemplate.Parse("nginx").CreatePattern(false, true, true);

            Assert.That(pattern.Group, Is.EqualTo("registry-1.docker.io/library/nginx:{v}"));
        }
    }
}
