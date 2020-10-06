using DockerUpgradeTool.Files;

namespace DockerUpgradeTool.Git
{
    public interface IRepositoryFileInfo
    {
        IFileInfo File { get; }
        bool Ignored { get; }
        string RelativePath { get; }
        IDirectoryInfo Root { get; }
    }
}
