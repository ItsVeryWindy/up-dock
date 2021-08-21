namespace UpDock.Files
{
    public interface IFileProvider
    {
        IDirectoryInfo CreateTemporaryDirectory();

        IDirectoryInfo GetDirectory(string path);

        IFileInfo CreateTemporaryFile();

        IFileInfo? GetFile(string? path);
    }
}
