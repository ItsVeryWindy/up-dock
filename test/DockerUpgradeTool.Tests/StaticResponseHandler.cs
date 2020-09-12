using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DockerUpgradeTool.Tests
{
    class StaticResponseHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stream = typeof(StaticResponseHandler).Assembly.GetManifestResourceStream("DockerUpgradeTool.Tests.tags_response.json");

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new StreamContent(stream)
            });
        }
    }
}
