﻿using System.Collections.Generic;
using NuGet.Versioning;

namespace DockerUpgradeTool.Imaging.Parts
{
    public interface IDockerImagePatternPart
    {
        IDockerImagePatternPart? Next { get; }

        void EnsureExecution(IEnumerable<NuGetVersion> versions);

        string Execute(IEnumerable<NuGetVersion> versions);

        string ToString();
    }
}