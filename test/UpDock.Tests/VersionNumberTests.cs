using NUnit.Framework;
using UpDock.Versioning;

namespace UpDock.Tests
{
    public class VersionNumberTests
    {
        [TestCase("1.0.0-alpha")]
        [TestCase("1.0.0-alpha.1")]
        [TestCase("1.0.0-alpha.beta")]
        [TestCase("1.0.0-beta")]
        [TestCase("1.0.0-beta.2")]
        [TestCase("1.0.0-beta.11")]
        [TestCase("1.0.0-rc.1")]
        [TestCase("1.0.0")]
        public void ShouldBeEqual(string versionStr)
        {
            var a = VersionNumber.Parse(versionStr);

            var b = VersionNumber.Parse(versionStr);

            Assert.That(a == b, Is.True);
            Assert.That(a >= b, Is.True);
            Assert.That(a <= b, Is.True);
        }

        [TestCase("1.0.0-alpha")]
        [TestCase("1.0.0-alpha.1")]
        [TestCase("1.0.0-alpha.beta")]
        [TestCase("1.0.0-beta")]
        [TestCase("1.0.0-beta.2")]
        [TestCase("1.0.0-beta.11")]
        [TestCase("1.0.0-rc.1")]
        [TestCase("1.0.0")]
        public void ShouldNotBeEqual(string versionStr)
        {
            var a = VersionNumber.Parse(versionStr);

            var b = VersionNumber.Parse(versionStr);

            Assert.That(a != b, Is.False);
            Assert.That(a > b, Is.False);
            Assert.That(a < b, Is.False);
        }

        [TestCase("1.0.0-alpha.1", "1.0.0-alpha")]
        [TestCase("1.0.0-alpha.beta", "1.0.0-alpha.1")]
        [TestCase("1.0.0-beta", "1.0.0-alpha.beta")]
        [TestCase("1.0.0-beta.2", "1.0.0-beta")]
        [TestCase("1.0.0-beta.11", "1.0.0-beta.2")]
        [TestCase("1.0.0-rc.1", "1.0.0-beta.11")]
        [TestCase("1.0.0", "1.0.0-rc.1")]
        [TestCase("2.0.0", "1.0.0")]
        [TestCase("1.1.0", "1.0.0")]
        [TestCase("1.0.1", "1.0.0")]
        public void ShouldBeGreater(string aStr, string bStr)
        {
            var a = VersionNumber.Parse(aStr);

            var b = VersionNumber.Parse(bStr);

            Assert.That(a > b, Is.True);
            Assert.That(a < b, Is.False);
            Assert.That(a <= b, Is.False);
        }

        [TestCase("1.0.0-alpha", "1.0.0-alpha.1")]
        [TestCase("1.0.0-alpha.1", "1.0.0-alpha.beta")]
        [TestCase("1.0.0-alpha.beta", "1.0.0-beta")]
        [TestCase("1.0.0-beta", "1.0.0-beta.2")]
        [TestCase("1.0.0-beta.2", "1.0.0-beta.11")]
        [TestCase("1.0.0-beta.11", "1.0.0-rc.1")]
        [TestCase("1.0.0-rc.1", "1.0.0")]
        [TestCase("1.0.0", "2.0.0")]
        [TestCase("1.0.0", "1.1.0")]
        [TestCase("1.0.0", "1.0.1")]
        public void ShouldBeLesser(string aStr, string bStr)
        {
            var a = VersionNumber.Parse(aStr);

            var b = VersionNumber.Parse(bStr);

            Assert.That(a < b, Is.True);
            Assert.That(a > b, Is.False);
            Assert.That(a >= b, Is.False);
        }
    }
}
