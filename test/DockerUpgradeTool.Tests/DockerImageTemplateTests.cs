using System;
using DockerUpgradeTool.Imaging;
using NUnit.Framework;

namespace DockerUpgradeTool.Tests
{
    public class DockerImageTemplateTests
    {
        [TestCase("nginx", "https://registry-1.docker.io/", "library/nginx", "nginx:{v}")]
        [TestCase("library/nginx", "https://registry-1.docker.io/", "library/nginx", "library/nginx:{v}")]
        [TestCase("repository.com/nginx", "https://repository.com/", "nginx", "repository.com/nginx:{v}")]
        [TestCase("nginx:_{v}", "https://registry-1.docker.io/", "library/nginx", "nginx:_{v}")]
        public void ShouldHandleValidString(string str, string expectedRepository, string expectedImage, string expectedPattern)
        {
            var template = DockerImageTemplate.Parse(str);

            Assert.That(template.Repository.ToString(), Is.EqualTo(expectedRepository));
            Assert.That(template.Image, Is.EqualTo(expectedImage));

            var pattern = template.CreatePattern(true, true);

            Assert.That(pattern.ToString(), Is.EqualTo(expectedPattern));
        }

        [TestCase("repositor_y.com/nginx", "The registry name should not contain underscores.")]
        [TestCase("repository.com:abcd/nginx", "The registry name is invalid")]
        [TestCase("NGINX", "The image name should only contain lowercase letters, digits, periods, underscores, or dashes.")]
        [TestCase("-nginx", "The image name should not begin with a period, an underscore or a dash.")]
        [TestCase(".nginx", "The image name should not begin with a period, an underscore or a dash.")]
        [TestCase("_nginx", "The image name should not begin with a period, an underscore or a dash.")]
        [TestCase("nginx-", "The image name should not end with a period, an underscore or a dash.")]
        [TestCase("nginx.", "The image name should not end with a period, an underscore or a dash.")]
        [TestCase("nginx_", "The image name should not end with a period, an underscore or a dash.")]
        [TestCase("nginx:{v", "The image tag is missing closing bracket.")]
        [TestCase("nginx:{vx1-2}", "The image tag contains an invalid version range.")]
        [TestCase("nginx:tag:tag", "The image name should only have one colon.")]
        [TestCase("nginx:-tag", "The image tag cannot start with a period or a dash.")]
        [TestCase("nginx:.tag", "The image tag cannot start with a period or a dash.")]
        public void ShouldHandleInvalidString(string str, string expectedError) => Assert.That(() => DockerImageTemplate.Parse(str), Throws.TypeOf<FormatException>().With.Message.EqualTo(expectedError));
    }
}
