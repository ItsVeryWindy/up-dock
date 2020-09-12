using System.IO;

namespace DockerUpgrader.Files
{
    public interface IFileInfo
    {
        void Delete();
        IDirectoryInfo? Parent { get; }
        string Path { get; }
        bool Exists { get; }
        Stream CreateWriteStream();
        Stream CreateReadStream();
        void Move(IFileInfo file);

        string MakeRelativePath(IDirectoryInfo directory);
    }
}
