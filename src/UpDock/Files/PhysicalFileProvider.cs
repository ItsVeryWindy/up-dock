using System.IO;

namespace UpDock.Files
{
    public class PhysicalFileProvider : IFileProvider
    {
        public IDirectoryInfo GetDirectory(string directory) => new PhysicalDirectoryInfo(new DirectoryInfo(directory));

        public IFileInfo? GetFile(string? path)
        {
            if (path is null)
                return null;

            return new PhysicalFileInfo(new FileInfo(path));
        }

        public IFileInfo CreateTemporaryFile()
        {
            var tempFileName = Path.GetTempFileName();

            return new PhysicalFileInfo(new FileInfo(tempFileName));
        }

        public IDirectoryInfo CreateTemporaryDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            return new PhysicalDirectoryInfo(Directory.CreateDirectory(path));
        }
    }
}
