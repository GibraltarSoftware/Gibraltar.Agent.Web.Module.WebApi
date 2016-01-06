using System;
using System.Collections;
using System.Net.Http;
using System.Security.Principal;
using System.Web;
using Loupe.Agent.Web.Module.Handlers;
using Loupe.Agent.Web.Module.Infrastructure;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.LogMessage_Handler
{
    public class WebApiTestBase
    {
        protected LogMessageHandler Target;
        protected IPrincipal FakeUser;
        protected IIdentity FakeIdentity;
        protected HttpContextBase HttpContext;
        protected string DefaultTestSessionId;
        protected string DefaultAgentSessionId;
        protected Hashtable ContextItems;

        [SetUp]
        public void BaseSetUp()
        {
            Target = new LogMessageHandler();

            HttpContext = Substitute.For<HttpContextBase>();


            FakeUser = Substitute.For<IPrincipal>();
            FakeIdentity = Substitute.For<IIdentity>();
            FakeIdentity.Name.Returns("");
            FakeUser.Identity.Returns(FakeIdentity);

            DefaultTestSessionId = Guid.Empty.ToString();
            DefaultAgentSessionId = "8C6005BE-D7A9-46C1-BE7C-49228903A540";

            ContextItems = new Hashtable();
            SetContextLoupeSessionId(DefaultTestSessionId);
            SetContextAgentSessionId(DefaultAgentSessionId);

            HttpContext.Items.Returns(ContextItems);
            HttpContext.User.Returns(FakeUser);
            HttpContext.Cache.Returns(HttpRuntime.Cache);
            Target.Context = HttpContext;
        }
        protected void SetContextLoupeSessionId(string value)
        {
            ContextItems[Constants.SessionId] = value;
        }

        protected void SetContextAgentSessionId(string value)
        {
            ContextItems[Constants.AgentSessionId] = value;
        }

        protected void ClearAgentSessionId()
        {
            SetContextAgentSessionId("");
        }

        protected HttpRequestMessage CreateRequestMethod(string body, HttpMethod method = null, string url = null)
        {
            if (method == null)
            {
                method = HttpMethod.Post;
            }

            if (url == null)
            {
                url = "http://www.test.com/loupe/log";
            }

            var message = new HttpRequestMessage(method, new Uri(url));
            if (body != null)
            {
                message.Content = new StringContent(body);
            }

            return message;
        }

        protected HttpResponseMessage ExecuteHandler(HttpRequestMessage requestMessage)
        {
            var client = new HttpClient(Target);
            var task = client.SendAsync(requestMessage);
            task.Wait();
            return task.Result;
        }

        protected HttpResponseMessage SendRequest(string body, HttpMethod method = null, string url = null)
        {
            var message = CreateRequestMethod(body, method, url);
            return ExecuteHandler(message);
        }

    }


}