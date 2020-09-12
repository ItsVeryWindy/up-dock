namespace DockerUpgradeTool.Files
{
    public interface IFileFilterFactory
    {
        IFileFilter Create(IConfigurationOptions options);
    }
}
