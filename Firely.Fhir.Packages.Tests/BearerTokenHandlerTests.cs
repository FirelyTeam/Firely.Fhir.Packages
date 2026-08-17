using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class BearerTokenHandlerTests
    {
        private class CapturingHandler : HttpMessageHandler
        {
            public HttpRequestMessage? LastRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });
            }
        }

        [TestMethod]
        public async Task AddsBearerTokenToRequest()
        {
            var inner = new CapturingHandler();
            using var client = new HttpClient(new BearerTokenHandler(() => Task.FromResult("my-token"), inner));

            await client.GetAsync("https://packages.example.org/feeds/private/mypackage/1.0.0");

            inner.LastRequest.Should().NotBeNull();
            inner.LastRequest!.Headers.Authorization.Should().NotBeNull();
            inner.LastRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
            inner.LastRequest.Headers.Authorization.Parameter.Should().Be("my-token");
        }

        [TestMethod]
        public async Task InvokesTokenProviderForEveryRequest()
        {
            var counter = 0;
            var inner = new CapturingHandler();
            using var client = new HttpClient(new BearerTokenHandler(() => Task.FromResult($"token-{++counter}"), inner));

            await client.GetAsync("https://packages.example.org/first");
            inner.LastRequest!.Headers.Authorization!.Parameter.Should().Be("token-1");

            await client.GetAsync("https://packages.example.org/second");
            inner.LastRequest!.Headers.Authorization!.Parameter.Should().Be("token-2");
        }

        [TestMethod]
        public async Task SkipsAuthorizationHeaderWhenTokenIsEmpty()
        {
            var inner = new CapturingHandler();
            using var client = new HttpClient(new BearerTokenHandler(() => Task.FromResult(string.Empty), inner));

            await client.GetAsync("https://packages.example.org/anything");

            inner.LastRequest!.Headers.Authorization.Should().BeNull();
        }

        [TestMethod]
        public void PackageClientCreateAcceptsTokenProvider()
        {
            using var client = (System.IDisposable)PackageClient.Create("https://packages.example.org/feeds/private",
                tokenProvider: () => Task.FromResult("my-token"));

            client.Should().NotBeNull();
        }
    }
}
