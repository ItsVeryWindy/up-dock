using System.Collections.Generic;

namespace DockerUpgradeTool.Files
{
    public interface IDirectoryInfo
    {
        IEnumerable<IFileInfo> Files { get; }
        string Path { get; }
        string Name { get; }
        IDirectoryInfo? Parent { get; }
    }
}
