namespace DockerUpgrader.Files
{
    public interface IFileProvider
    {
        IDirectoryInfo GetDirectory(string path);

        IFileInfo CreateTemporaryFile();

        IFileInfo? GetFile(string path);
    }
}
