using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Loupe.Agent.Web.Module.Handlers;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests
{
    [TestFixture]
    public class When_init_module
    {
        [Test]
        public void Should_sucessfully_init_module()
        {
            var loggingModule = new Loupe.Agent.Web.Module.Logging();
            var application = new HttpApplication();

            Assert.DoesNotThrow(() => loggingModule.Init(application));
        }

        [Test]
        public void Should_create_message_handler()
        {
            var loggingModule = new Loupe.Agent.Web.Module.Logging();
            var application = new HttpApplication();

            loggingModule.Init(application);

            var hasLogMessageHandler = GlobalConfiguration.Configuration.MessageHandlers.Any(x => x.GetType() == typeof(LogMessageHandler));

            Assert.That(hasLogMessageHandler, Is.True);

        }

    }
}
