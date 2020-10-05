﻿using System.Collections.Generic;
using DockerUpgradeTool.Imaging;

namespace DockerUpgradeTool
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

        IConfigurationOptions Merge(IConfigurationOptions options);
    }
}