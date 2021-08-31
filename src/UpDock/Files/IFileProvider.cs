using System.Diagnostics.CodeAnalysis;

namespace UpDock.Files
{
    public interface IFileProvider
    {
        IDirectoryInfo CreateTemporaryDirectory();

        IDirectoryInfo GetDirectory(string path);

        IFileInfo CreateTemporaryFile();

        [return: NotNullIfNotNull("path")]
        IFileInfo? GetFile(string? path);
    }
}
