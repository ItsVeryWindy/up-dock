using System.Collections.Generic;
using System.IO;

namespace UpDock.Files
{
    public interface IDirectoryInfo
    {
        IEnumerable<IFileInfo> AllFiles { get; }
        string AbsolutePath { get; }
        string Name { get; }
        IDirectoryInfo? Parent { get; }
        bool Exists { get; }
        IEnumerable<IFileInfo> Files { get; }
        IEnumerable<IDirectoryInfo> Directories { get; }

        IFileInfo GetFile(string relativePath);
        void Delete();
        IDirectoryInfo SetAttributes(FileAttributes fileAttributes);
        IDirectoryInfo Create();
    }
}
