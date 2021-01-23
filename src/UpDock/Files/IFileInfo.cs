using System.IO;

namespace UpDock.Files
{
    public interface IFileInfo
    {
        void Delete();
        IDirectoryInfo? Parent { get; }
        string AbsolutePath { get; }
        bool Exists { get; }
        Stream CreateWriteStream();
        Stream? CreateReadStream();
        void Move(IFileInfo file);
    }
}
