using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using DockerUpgrader.Files;
using DockerUpgrader.Git;
using DockerUpgrader.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Internal;

namespace DockerUpgrader
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<CommandLineOptions>(args);

            return result.WithParsedAsync(Execute);
        }

        private static async Task Execute(CommandLineOptions options)
        {
            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, args) => { cts.Cancel(); };

            var services = CreateServices(options, cts.Token).BuildServiceProvider();

            await services.GetRequiredService<IGitRepositoryProcessor>().ProcessAsync();
        }

        public static IServiceCollection CreateServices(CommandLineOptions options, CancellationToken cancellationToken)
        {
            return new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddSingleton<HttpClient>()
                .AddSingleton<IVersionCache, VersionCache>()
                .AddSingleton(options)
                .AddSingleton<IFileProvider, PhysicalFileProvider>()
                .AddSingleton<ICancellationProvider>(new CancellationProvider(cancellationToken))
                .AddSingleton<IGitHubClient>(sp => new GitHubClient(
                    new ProductHeaderValue("docker-upgrade-tool", "1.0.0"),
                    new InMemoryCredentialStore(new Credentials(sp.GetRequiredService<IConfigurationOptions>().Token,
                        AuthenticationType.Bearer))))
                .AddSingleton<IGitRepositoryFactory, GitRepositoryFactory>()
                .AddSingleton<IReplacementPlanner, ReplacementPlanner>()
                .AddSingleton<IReplacementPlanExecutor, ReplacementPlanExecutor>()
                .AddSingleton<IFileFilterFactory, FileFilterFactory>()
                .AddSingleton<IGitRepositoryProcessor, GitRepositoryProcessor>()
                .AddSingleton<IConfigurationOptions>(sp => sp.GetRequiredService<IOptions<ConfigurationOptions>>().Value)
                .ConfigureOptions<ConfigureCommandLineOptions>()
                .AddOptions<ConfigurationOptions>()
                .Services;
        }
    }
}
