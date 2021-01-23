using System.Collections.Generic;

namespace UpDock.Files
{
    public interface IDirectoryInfo
    {
        IEnumerable<IFileInfo> Files { get; }
        string AbsolutePath { get; }
        string Name { get; }
        IDirectoryInfo? Parent { get; }
        IFileInfo GetFile(string relativePath);
    }
}
