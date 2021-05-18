// <copyright file="VersionController.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace argonaut_subscription_client_host.Controllers
{
    /// <summary>A controller processing version requests.
    /// Responds to:
    ///     GET:    /api/
    ///     GET:    /api/version/
    /// </summary>
    [Produces("application/json")]
    public class VersionController : Controller
    {
        private const string _configPrefix = "Client_";

        /// <summary>Information about the route.</summary>
        private class RouteInfo {
            public string FunctionName { get; set; }
            public string ControllerName { get; set; }
            public string UriTemplate { get; set; }
        }

        /// <summary>   The configuration. </summary>
        private readonly IConfiguration _config;

        /// <summary>The provider.</summary>
        private readonly IActionDescriptorCollectionProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionController"/> class.
        /// </summary>
        /// <param name="iConfiguration">Reference to the injected configuration object.</param>
        /// <param name="provider">      The provider.</param>
        public VersionController(
            IConfiguration iConfiguration, 
            IActionDescriptorCollectionProvider provider)
        {
            _config = iConfiguration;
            _provider = provider;
        }
    
        /// <summary>(An Action that handles HTTP GET requests) gets version information.</summary>
        /// <returns>The version information.</returns>
        [HttpGet]
        [Route("/api/")]
        [Route("/api/version/")]
        public virtual IActionResult GetVersionInfo()
        {
            // create a basic tuple to return
            Dictionary<string, string> information = new Dictionary<string, string>();

            information.Add("Application", AppDomain.CurrentDomain.FriendlyName);
            information.Add("Runtime", Environment.Version.ToString());

            // get the file version of the assembly that launched us
            information.Add("Version", FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).FileVersion.ToString());

            // add the list of configuration keys and values
            IEnumerable<IConfigurationSection> configItems = _config.GetChildren();

            foreach (IConfigurationSection configItem in configItems)
            {
                if (configItem.Key.StartsWith(_configPrefix, StringComparison.Ordinal))
                {
                    information.Add(configItem.Key, configItem.Value);
                }
            }

            // try to get a list of routes
            try
            {
                List<RouteInfo> routes = _provider.ActionDescriptors.Items.Select(x => new RouteInfo()
                        {
                            FunctionName = x.RouteValues["Action"],
                            ControllerName = x.RouteValues["Controller"],
                            UriTemplate = x.AttributeRouteInfo.Template
                        })
                    .ToList();

                information.Add("Routes", JsonConvert.SerializeObject(routes));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return StatusCode((int)HttpStatusCode.OK, information);
        }
    }
}
