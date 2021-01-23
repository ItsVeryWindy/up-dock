namespace UpDock.Files
{
    public class FileFilterFactory : IFileFilterFactory
    {
        public IFileFilter Create(IConfigurationOptions options) => new FileFilter(options);
    }
}
