using System.Net;
using System.Net.Http;
using System.Text;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.LogMessage_Handler
{
    [TestFixture]
    public class When_validating_request:WebApiTestBase
    {
        [Test]
        public void Should_only_handle_POST([Values("GET", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS")] string methodVerb)
        {
            var method = new HttpMethod(methodVerb);
            var response = SendRequest(null,method);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.MethodNotAllowed));
        }

        [Test]
        public void Should_return_400_if_no_content_in_stream()
        {
            var response = SendRequest(null);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public void Should_return_400_if_content_exceeds_2k()
        {

            StringBuilder body = new StringBuilder();

            for (int i = 0; i < 204801; i++)
            {
                body.Append('X');
            }

            var response = SendRequest(body.ToString());

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.RequestEntityTooLarge));
        }
    }
}