using System.Collections.Generic;
using DockerUpgrader.Imaging;

namespace DockerUpgrader
{
    public interface IConfigurationOptions
    {
        IReadOnlyCollection<string> Include { get; }
        IReadOnlyCollection<string> Exclude { get; }
        IReadOnlyCollection<DockerImageTemplatePattern> Patterns { get; }
        string Search { get; }
        string Token { get; }
        IReadOnlyDictionary<string, AuthenticationOptions> Authentication { get; }

        IConfigurationOptions Merge(IConfigurationOptions options);
    }
}