using System;
using UpDock.Imaging;
using NUnit.Framework;

namespace UpDock.Tests
{
    public class DockerImageTemplateTests
    {
        [TestCase("nginx", "https://registry-1.docker.io/", "library/nginx", false, "{v*}", "nginx:{v*}")]
        [TestCase("library/nginx", "https://registry-1.docker.io/", "library/nginx", false, "{v*}", "library/nginx:{v*}")]
        [TestCase("repository.com/nginx", "https://repository.com/", "nginx", false, "{v*}", "repository.com/nginx:{v*}")]
        [TestCase("nginx:_{v}", "https://registry-1.docker.io/", "library/nginx", false, "_{v*}", "nginx:_{v*}")]
        [TestCase("nginx@{digest}", "https://registry-1.docker.io/", "library/nginx", true, "{v*}", "nginx@{digest}")]
        [TestCase("nginx@{digest}:{v1.*}", "https://registry-1.docker.io/", "library/nginx", true, "{v1.*}", "nginx@{digest}")]

        public void ShouldHandleValidString(string str, string expectedRepository, string expectedImage, bool hasDigest, string expectedTag, string expectedPattern)
        {
            var template = DockerImageTemplate.Parse(str);

            Assert.That(template.Repository.ToString(), Is.EqualTo(expectedRepository));
            Assert.That(template.Image, Is.EqualTo(expectedImage));
            Assert.That(template.HasDigest, Is.EqualTo(hasDigest));
            Assert.That(template.Tag, Is.EqualTo(expectedTag));

            var pattern = template.CreatePattern(true, true, true, false, true);

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
        [TestCase("nginx@:@", "The image name for a template should only have one @ symbol.")]
        [TestCase("nginx@hello", "If an @ symbol is specified in the template, it must be followed by '{digest}' to indicate that a digest is required.")]
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
            var pattern = DockerImageTemplate.Parse("nginx").CreatePattern(false, true, true, false, true);

            Assert.That(pattern.Group, Is.EqualTo("registry-1.docker.io/library/nginx:{v}"));
        }
    }
}
