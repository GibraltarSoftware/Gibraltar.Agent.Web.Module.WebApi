using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;


namespace Loupe.Agent.Web.Module
{
    public class LoupeController : ApiController
    {
        //[Route("loupe/log")]
        [HttpPost]
        public HttpResponseMessage Log(JObject message)
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        //[Route("loupe/data", Name = "LoupeData")]
        public HttpResponseMessage Data(JObject someData)
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Got Data");
        }
    }
}