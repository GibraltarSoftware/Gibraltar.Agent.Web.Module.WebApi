using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Loupe.Agent.Web.Module.Handlers;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.LogMessage_Handler
{
    [TestFixture]
    public class When_receives_unrelated_request
    {

        [Test]
        public void Should_pass_request_on([Values("http://www.test.com/", 
                                                   "http://www.test.com/Gibraltar", 
                                                   "http://www.test.com/Gibraltar/log/things",
                                                   "http://www.test.com/gibraltar/data")] string url)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            var handler = new LogMessageHandler
            {
                InnerHandler = new TestHandler((r, c) =>
                {
                    return TestHandler.Return200();
                })
            };

            var client = new HttpClient(handler);
            var response = client.SendAsync(httpRequestMessage).Result;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.ReasonPhrase, Is.EqualTo("From inner handler"));
        }


        public class TestHandler : DelegatingHandler
        {
            private readonly Func<HttpRequestMessage,
                CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

            public TestHandler()
            {
                _handlerFunc = (r, c) => Return200();
            }

            public TestHandler(Func<HttpRequestMessage,
                CancellationToken, Task<HttpResponseMessage>> handlerFunc)
            {
                _handlerFunc = handlerFunc;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _handlerFunc(request, cancellationToken);
            }

            public static Task<HttpResponseMessage> Return200()
            {
                return Task.Factory.StartNew(
                    () =>
                    {
                        var message = new HttpResponseMessage(HttpStatusCode.OK);
                        message.ReasonPhrase = "From inner handler";
                        return message;
                    });
            }
        }

    }
}