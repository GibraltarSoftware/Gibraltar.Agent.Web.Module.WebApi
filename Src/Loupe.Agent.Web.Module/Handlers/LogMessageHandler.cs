using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using Gibraltar.Agent;
using Loupe.Agent.Web.Module.DetailBuilders;
using Loupe.Agent.Web.Module.Infrastructure;
using Loupe.Agent.Web.Module.Models;
using Newtonsoft.Json;
using Exception = System.Exception;

namespace Loupe.Agent.Web.Module.Handlers
{
    public class LogMessageHandler : DelegatingHandler
    {
        private const string LogSystem = "Loupe";
        private const string Category = "Loupe.Internal";

        private readonly UrlCheck _urlCheck;
        private JavaScriptLogger _javaScriptLogger;
        private HttpResponseMessage _responseMessage;
        private HttpContextBase _context;


        internal HttpContextBase Context
        {
            get { return _context ?? (_context = new HttpContextWrapper(HttpContext.Current)); }
            set { _context = value; }
        }

        /// <summary>
        /// Allows tests to inject mock class for testing purposes
        /// </summary>
        internal JavaScriptLogger JavaScriptLogger
        {
            get { return _javaScriptLogger ?? (_javaScriptLogger = new JavaScriptLogger()); }
            set { _javaScriptLogger = value; }
        }

        public LogMessageHandler()
        {
            _urlCheck = new UrlCheck();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                if (_urlCheck.IsLoupeUrl(request))
                {
                    if (RequestIsValid(request))
                    {
                        if (await LogMessage(request))
                        {
                            CreateResponseMessage(request, HttpStatusCode.NoContent);
                        }
                        else
                        {
                            if (_responseMessage == null)
                            {
                                CreateResponseMessage(request, HttpStatusCode.BadRequest, "Failed to log details provided.");
                            }
                        }
                    }

                    return _responseMessage;
                }
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);
#if DEBUG
                Log.Write(LogMessageSeverity.Critical, LogSystem, 0, ex, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(request), Category, "Unable to process message due to " + ex.GetType(),
                    "Exception caught in top level catch block, this should have be caught by error handler specific to the part of the request processing that failed.");

                CreateResponseMessage(request, HttpStatusCode.InternalServerError, "Error whilst attempting to process Loupe log request");

                return _responseMessage;
#endif
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<bool> LogMessage(HttpRequestMessage request)
        {
            var logRequest = await GetMessageFromRequestBody(request);

            if (logRequest != null)
            {
                CacheClientDetails(logRequest);
                logRequest.User = Context.User;
                AddSessionId(request, logRequest);

                try
                {
                    JavaScriptLogger.Log(logRequest);

                    CreateResponseMessage(request, HttpStatusCode.NoContent);

                    return true;
                }
                catch (Exception ex)
                {
                    GC.KeepAlive(ex);
#if DEBUG
                    Log.Write(LogMessageSeverity.Error, LogSystem, 0, ex, LogWriteMode.Queued, 
                        CreateStandardRequestDetailXml(request), Category, "Error writing log to Loupe",
                        "An exception occurred whilst attempting to record the LogRequest in Loupe");
#endif
                    CreateResponseMessage(request, HttpStatusCode.InternalServerError, "Unable to record log request");
                }
            }

            return false;
        }

        private void AddSessionId(HttpRequestMessage request, LogRequest logRequest)
        {
            var sessionId = Context.Items[Constants.SessionId] as string;
            var agentSessionId = Context.Items[Constants.AgentSessionId] as string;

            if (string.IsNullOrWhiteSpace(agentSessionId) && logRequest.Session != null &&
                !string.IsNullOrWhiteSpace(logRequest.Session.CurrentAgentSessionId))
            {

                Context.Items[Constants.AgentSessionId] = logRequest.Session.CurrentAgentSessionId;
            }

            for (int i = 0; i < logRequest.LogMessages.Count; i++)
            {
                var message = logRequest.LogMessages[i];

                if (sessionId == null && string.IsNullOrWhiteSpace(message.SessionId))
                {
#if DEBUG
                    Log.Write(LogMessageSeverity.Warning, LogSystem, 0, null, LogWriteMode.Queued,
                        CreateStandardRequestDetailXml(request), Category, "No session Id",
                        "No session in context for Loupe or explicit session Id found on the request, not able to set session Id");
#endif
                    continue;
                }

                if (string.IsNullOrWhiteSpace(message.SessionId))
                {
                    message.SessionId = sessionId;
                }

                if (string.IsNullOrWhiteSpace(message.AgentSessionId))
                {
                    message.AgentSessionId = agentSessionId;
                }

            }
        }

        private void CacheClientDetails(LogRequest logRequest)
        {
            var sessionId = Context.Items[Constants.SessionId] as string;
            if (sessionId != null && logRequest.Session != null && logRequest.Session.Client != null)
            {
                var clientDetailsBuilder = new ClientDetailsBuilder();

                Context.Cache.Insert(sessionId, clientDetailsBuilder.Build(logRequest), null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(10));
            }
        }

        private async Task<LogRequest> GetMessageFromRequestBody(HttpRequestMessage request)
        {
            var requestBody = await ReadInputStream(request);

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return null;
            }

            return DeserializeBody(request, requestBody);
        }

        private async Task<string> ReadInputStream(HttpRequestMessage request)
        {
            try
            {
                return await request.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);
                Log.Write(LogMessageSeverity.Error, LogSystem, 0, ex, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(request), Category, "Error reading input stream",
                    "An exception occurred whilst attempting to read the request input stream to enable deserialization of the data");
            }

            return null;
        }

        private LogRequest DeserializeBody(HttpRequestMessage request, string body)
        {
            LogRequest requestBody = null;

            try
            {
                requestBody = JsonConvert.DeserializeObject<LogRequest>(body, new JsonSerializerSettings());
            }
            catch (Exception ex)
            {
                GC.KeepAlive(ex);

                Log.Write(LogMessageSeverity.Error, LogSystem, 0, ex, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(request, body), Category, "Error deserializing request body",
                    "An exception occurred whilst attempting to deserialize the request body");

                CreateResponseMessage(request, HttpStatusCode.BadRequest, "Problem encountered deserializing data");
            }

            return requestBody;
        }

        private bool RequestIsValid(HttpRequestMessage request)
        {
            if (!ValidateMethod(request)) return false;

            if (!ValidateContent(request)) return false;

            return true;
        }

        private void CreateResponseMessage(HttpRequestMessage request, HttpStatusCode statusCode, string userDescription = null)
        {
            var response = request.CreateResponse(statusCode);

            if (!string.IsNullOrWhiteSpace(userDescription))
            {
                response.ReasonPhrase = userDescription;
            }

            _responseMessage = response;
        }

        private bool ValidateContent(HttpRequestMessage request)
        {

            if (request.Content == null)
            {

#if DEBUG
                Log.Write(LogMessageSeverity.Information, LogSystem, 0, null, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(request), Category, "Null content",
                    "Request was received but had no content");
#endif

                CreateResponseMessage(request, HttpStatusCode.BadRequest, "Null request body");
                return false;
            }

            long? length = request.Content.Headers.ContentLength;

            if (length == 0)
            {
                // No request body return bad request
#if DEBUG
                Log.Write(LogMessageSeverity.Information, LogSystem, 0, null, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(request), Category, "Empty request body",
                    "Request was received for but no body was included in the POST");
#endif

                CreateResponseMessage(request, HttpStatusCode.BadRequest, "Empty request body");
                return false;
            }

            if (length > 204800)
            {
#if DEBUG
                Log.Write(LogMessageSeverity.Information, LogSystem, 0, null, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(request), Category, "Request body exceeds limit",
                    "Request was received but the body included exceeded the size limit of 2k and was {0}", SizeSuffix(length.Value));
#endif
                CreateResponseMessage(request, HttpStatusCode.RequestEntityTooLarge, "Request body exceeds size limit");
                return false;
            }

            return true;
        }

        private bool ValidateMethod(HttpRequestMessage request)
        {
            if (request.Method.Method.ToUpperInvariant() != "POST")
            {
                // We have received a request which is specifically for us but on a method we don't support.
                // Record that this has happened and then let the request carry on and host application
                // deal with an invalid request
#if DEBUG
                Log.Write(LogMessageSeverity.Warning, LogSystem, 0, null, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(request), Category, "Invalid HttpMethod",
                    "Received request but was a {0} rather than a POST", request.Method.Method);
#endif

                CreateResponseMessage(request, HttpStatusCode.MethodNotAllowed, request.Method.Method + " not allowed. Use POST");
                return false;
            }

            return true;
        }

        private static string CreateStandardRequestDetailXml(HttpRequestMessage request, string requestBody = null)
        {
            var builder = new RequestBlockBuilder(new WebApiRequestDetailBuilder(request));

            return builder.Build(requestBody);
        }

        private readonly string[] _sizeSuffixes = { "bytes", "KB", "MB", "GB" };
        private string SizeSuffix(long value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, _sizeSuffixes[mag]);
        }
    }
}