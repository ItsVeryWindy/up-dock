using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DockerUpgrader.Files
{
    public class PhysicalFileProvider : IFileProvider
    {
        public IDirectoryInfo GetDirectory(string directory)
        {
            return new PhysicalDirectoryInfo(new DirectoryInfo(directory));
        }

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

    public class PhysicalDirectoryInfo : IDirectoryInfo
    {
        private readonly DirectoryInfo _directory;

        public PhysicalDirectoryInfo(DirectoryInfo directory)
        {
            _directory = directory;
        }

        public IEnumerable<IFileInfo> Files => _directory.GetFiles("*", SearchOption.AllDirectories).Select(x => new PhysicalFileInfo(x));

        public string Path => _directory.FullName;

        public string Name => _directory.Name;

        public IDirectoryInfo? Parent => _directory.Parent == null ? null : new PhysicalDirectoryInfo(_directory.Parent);
    }
}
