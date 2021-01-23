using System.IO;

namespace UpDock.Files
{
    public class PhysicalFileProvider : IFileProvider
    {
        public IDirectoryInfo GetDirectory(string directory) => new PhysicalDirectoryInfo(new DirectoryInfo(directory));

        public IFileInfo? GetFile(string path)
        {
            if (path == null)
                return null;

            return new PhysicalFileInfo(new FileInfo(path));
        }

        public IFileInfo CreateTemporaryFile()
        {
            var tempFileName = Path.GetTempFileName();

            return new PhysicalFileInfo(new FileInfo(tempFileName));
        }
    }
}
