namespace DockerUpgrader.Files
{
    public class FileFilterFactory : IFileFilterFactory
    {
        public IFileFilter Create(IConfigurationOptions options)
        {
            return new FileFilter(options);
        }
    }
}
