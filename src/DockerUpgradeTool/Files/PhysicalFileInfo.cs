using System;
using System.IO;

namespace DockerUpgradeTool.Files
{
    public class PhysicalFileInfo : IFileInfo
    {
        private readonly FileInfo _file;

        public IDirectoryInfo? Parent => new PhysicalDirectoryInfo(_file.Directory);
        public string Name => _file.Name;
        public string AbsolutePath => _file.FullName;
        public bool Exists => _file.Exists;

        public PhysicalFileInfo(FileInfo file)
        {
            _file = file;
        }

        public void Delete() => _file.Delete();

        public Stream CreateWriteStream() => new FileStream(AbsolutePath, FileMode.Truncate, FileAccess.Write);

        public Stream? CreateReadStream()
        {
            try
            {
                return new FileStream(AbsolutePath, FileMode.Open, FileAccess.Read);
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
        }

        public void Move(IFileInfo file) => File.Move(AbsolutePath, file.AbsolutePath);
    }
}
