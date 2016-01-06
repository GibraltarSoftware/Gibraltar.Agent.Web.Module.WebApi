#region File Header

// <copyright file="Logging.cs" company="Gibraltar Software Inc.">
// Gibraltar Software Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Loupe.Agent.Web.Module.Handlers;

#endregion

namespace Loupe.Agent.Web.Module
{
    public class Logging:IHttpModule
    {
        private CORSHandler _corsHandler;
        private MessageHandler _messageHandler;
        private CookieHandler _cookieHandler;
        private HeaderHandler _headerHandler;
        private static volatile bool _applicationStarted = false;
        private static readonly object ApplicationStartLock = new object();

        public void Init(HttpApplication application)
        {
            _corsHandler = new CORSHandler();
            //_messageHandler = new MessageHandler();
            _cookieHandler = new CookieHandler();
            _headerHandler = new HeaderHandler();
            OnStart();
            //application.PostAuthenticateRequest += OnPostAuthenticateRequest;
            application.BeginRequest += application_BeginRequest;
        }


        private void OnStart()
        {
            if (!_applicationStarted)
            {
                lock (ApplicationStartLock)
                {
                    if (!_applicationStarted)
                    {
                        // this will run only once per application start
                        GlobalConfiguration.Configuration.MessageHandlers.Add(new LogMessageHandler());
                        //GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                        //    name: "LoupeData",
                        //    routeTemplate: "loupe/logging/log",
                        //    defaults: new { controller = "Loupe", action = "data" });

                        //GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                        //    name: "LoupeLogging",
                        //    routeTemplate: "api/loupe/log");
                        _applicationStarted = true;
                    }
                }
            }
        }

        void application_BeginRequest(object sender, EventArgs e)
        {
            var context = new HttpContextWrapper(((HttpApplication)sender).Context);

            if (_corsHandler.HandleRequest(context))
            {
                ((HttpApplication)sender).CompleteRequest();
            }
            else
            {
                _cookieHandler.HandleRequest(context);
                _headerHandler.HandleRequest(context);
            }
        }

        private void OnPostAuthenticateRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication) sender;

            var handled = _messageHandler.HandleRequest(new HttpContextWrapper(application.Context));

            if (handled)
            {
               application.CompleteRequest();
            }
        }

        // Implemented as part of IHttpModule
        // but we don't use it
        public void Dispose() { }
    }
}
