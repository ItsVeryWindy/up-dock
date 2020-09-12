using System.Threading.Tasks;

namespace DockerUpgrader
{
    public interface IGitRepositoryProcessor
    {
        Task ProcessAsync();
    }
}
