using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DockerUpgrader.Tests
{
    class StaticResponseHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stream = typeof(StaticResponseHandler).Assembly.GetManifestResourceStream("DockerUpgrader.Tests.tags_response.json");

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new StreamContent(stream)
            });
        }
    }
}
