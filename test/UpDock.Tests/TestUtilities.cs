using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace UpDock.Tests
{
    public static class TestUtilities
    {
        public static IServiceCollection CreateServices()
        {
            var services = new ServiceCollection();

            Program.ConfigureServices(services);

            return services;
        }

        public static Stream GetResource(string name) => typeof(ReplacementPlanExecutorTests).Assembly.GetManifestResourceStream($"UpDock.Tests.{name}")!;

        public static async Task<string> GetStringAsync(this Stream? stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using var sr = new StreamReader(stream);

            return await sr.ReadToEndAsync();
        }
    }
}
