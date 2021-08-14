using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Imaging;

namespace UpDock.Tests
{
    internal class StaticResponseHandler : DelegatingHandler
    {
        private static readonly Uri AuthenticationUri = new("https://auth.docker.io/token?service=registry.docker.io&scope=repository%3alibrary%2fimage%3apull");

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(request.RequestUri is null)
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    RequestMessage = request
                });

            if(request.RequestUri == AuthenticationUri)
            {
                var stream = typeof(StaticResponseHandler).Assembly.GetManifestResourceStream("UpDock.Tests.token_response.json");

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(stream!)
                });
            }

            if (request.Headers.Authorization is null)
            {
                if (request.RequestUri.Host == DockerImageTemplate.DefaultRepository.Host || request.RequestUri.Host == "awsaccountid.dkr.ecr.region.amazonaws.com")
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        RequestMessage = request
                    };

                    if (request.RequestUri.Host == DockerImageTemplate.DefaultRepository.Host)
                    {
                        response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer", "realm=\"https://auth.docker.io/token\",service=\"registry.docker.io\",scope=\"repository:library/image:pull\""));
                    }

                    return Task.FromResult(response);
                }
            }

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
