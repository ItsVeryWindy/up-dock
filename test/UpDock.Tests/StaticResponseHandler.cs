using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UpDock.Tests
{
    internal class StaticResponseHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath.Contains("tags") == true)
            {
                var stream = typeof(StaticResponseHandler).Assembly.GetManifestResourceStream("UpDock.Tests.tags_response.json");

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(stream!)
                });
            }

            if (request.RequestUri?.AbsolutePath.Contains("manifest") == true)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Headers = {
                        { "Docker-Content-Digest", "sha256:4f880368ed63767483b6f6c5bf7efde3af3faba816e71ff42db50326b0386bed" }
                    }
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
