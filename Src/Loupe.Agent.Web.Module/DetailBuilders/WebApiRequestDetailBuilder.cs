using System;
using System.Collections;
using System.Net.Http;
using System.Web;

namespace Loupe.Agent.Web.Module.DetailBuilders
{
    public class WebApiRequestDetailBuilder : IRequestDetailBuilder
    {
        private readonly HttpRequestMessage _requestMessage;

        public WebApiRequestDetailBuilder(HttpRequestMessage request)
        {
            _requestMessage = request;
        }

        public RequestBlockDetail GetDetails()
        {
            var requestHeaders = _requestMessage.Headers;
            string contentType = "";
            long contentLength = 0;
            if (_requestMessage.Content != null)
            {
                contentType = _requestMessage.Content.Headers.ContentType.ToString();
                contentLength = _requestMessage.Content.Headers.ContentLength ?? 0;
            }
            var userAgent = requestHeaders.UserAgent;
            var userBrowser = new HttpBrowserCapabilities { Capabilities = new Hashtable { { string.Empty, userAgent } } };
            var isSecure = _requestMessage.RequestUri.Scheme == Uri.UriSchemeHttps;

            return new RequestBlockDetail(userBrowser.Browser,
                contentType,
                contentLength,
                _requestMessage.IsLocal(),
                isSecure,
                GetClientIPAddress(),
                "");
        }

        private string GetClientIPAddress()
        {
            const string HttpContext = "MS_HttpContext";

            if (_requestMessage.Properties.ContainsKey(HttpContext))
            {
                dynamic ctx = _requestMessage.Properties[HttpContext];
                if (ctx != null)
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            return string.Empty;
        }
    }
}