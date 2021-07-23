using System.Net.Http;
using System.Threading.Tasks;
using UpDock.CommandLine;
using UpDock.Files;
using UpDock.Git;
using UpDock.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Internal;
using UpDock.Caching;

namespace UpDock
{
    public class Program
    {
        public static Task Main(string[] args) => CommandLineRunner.RunAsync<CommandLineOptions, CommandLineOptionsRunner>(ConfigureServices, ConfigureLogging, args);

        public static void ConfigureLogging(ILoggingBuilder builder) => builder.AddConsole();

        public static void ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogging(x => x.AddConsole())
                .AddSingleton<HttpClient>()
                .AddSingleton<IVersionCache, VersionCache>()
                .AddSingleton<IUpdateCache, UpdateCache>()
                .AddSingleton<IRepositorySearcher, GitHubRepositorySearcher>()
                .AddSingleton<IFileProvider, PhysicalFileProvider>()
                .AddSingleton<IGitHubClient>(sp => {
                    var token = sp.GetRequiredService<IConfigurationOptions>().Token;

                    var processInfo = sp.GetRequiredService<IProcessInfo>();

                    return new GitHubClient(
                        new ProductHeaderValue("up-dock", processInfo.Version),
                        token == null
                        ? new InMemoryCredentialStore(Credentials.Anonymous)
                        : new InMemoryCredentialStore(new Credentials(token, AuthenticationType.Bearer)));
                })
                .AddSingleton<IGitRepositoryFactory, GitRepositoryFactory>()
                .AddSingleton<IReplacementPlanner, ReplacementPlanner>()
                .AddSingleton<IReplacementPlanExecutor, ReplacementPlanExecutor>()
                .AddSingleton<IFileFilterFactory, FileFilterFactory>()
                .AddSingleton<IGitRepositoryProcessor, GitRepositoryProcessor>()
                .AddSingleton<IConfigurationOptions>(sp => sp.GetRequiredService<IOptions<ConfigurationOptions>>().Value)
                .ConfigureOptions<ConfigureCommandLineOptions>()
                .AddOptions<ConfigurationOptions>();
        }
    }
}
