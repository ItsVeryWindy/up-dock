using System.Collections.Generic;
using UpDock.Imaging;

namespace UpDock
{
    public interface IConfigurationOptions
    {
        IReadOnlyCollection<string> Include { get; }
        IReadOnlyCollection<string> Exclude { get; }
        IReadOnlyCollection<DockerImageTemplatePattern> Patterns { get; }
        string? Search { get; }
        string? Token { get; }
        IReadOnlyDictionary<string, AuthenticationOptions> Authentication { get; }
        bool DryRun { get; }
        bool AllowDowngrade { get; }

        IConfigurationOptions Merge(IConfigurationOptions options);
        string CreateHash();
    }
}
