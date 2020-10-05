using System.IO;

namespace DockerUpgradeTool.Files
{
    public interface IFileInfo
    {
        void Delete();
        IDirectoryInfo? Parent { get; }
        IDirectoryInfo? Root { get; }
        string Path { get; }
        bool Exists { get; }
        Stream CreateWriteStream();
        Stream CreateReadStream();
        void Move(IFileInfo file);

        string MakeRelativePath(IDirectoryInfo directory);
    }
}
