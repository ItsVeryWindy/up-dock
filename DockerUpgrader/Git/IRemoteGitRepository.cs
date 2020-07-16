using System.Threading.Tasks;

namespace DockerUpgrader.Git
{
    public interface IRemoteGitRepository
    {
        string CloneUrl { get; }
        string Owner { get; }
        string Name { get; }
        string Branch { get; }

        public Task<IRemoteGitRepository> ForkRepositoryAsync();

        public ILocalGitRepository CheckoutRepository();
    }
}
