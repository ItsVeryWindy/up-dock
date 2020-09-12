namespace DockerUpgrader.Files
{
    public interface IFileFilterFactory
    {
        IFileFilter Create(IConfigurationOptions options);
    }
}