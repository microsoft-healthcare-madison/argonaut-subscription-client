using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace argonaut_subscription_client_host.Controllers
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>A controller processing version requests.
    /// Responds to:
    ///     GET:    /api/
    ///     GET:    /api/version/
    /// </summary>
    ///
    /// <remarks>Gino Canessa, 7/18/2019.</remarks>
    ///-------------------------------------------------------------------------------------------------

    [Produces("application/json")]
    public class VersionController : Controller
    {
        #region Class Variables . . .

        #endregion Class Variables . . .

        #region Instance Variables . . .

        /// <summary>   The configuration. </summary>
        private readonly IConfiguration _config;

        private readonly IActionDescriptorCollectionProvider _provider;

        #endregion Instance Variables . . .

        #region Constructors . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Static constructor.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        static VersionController()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Constructor.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <param name="iConfiguration">Reference to the injected configuration object</param>
        /// <param name="provider">      The provider.</param>
        ///-------------------------------------------------------------------------------------------------

        public VersionController(
                                IConfiguration iConfiguration, 
                                IActionDescriptorCollectionProvider provider
                                )
        {
            // **** grab a reference to our application configuration ****

            _config = iConfiguration;
            _provider = provider;
        }

        #endregion Constructors . . .

        #region Class Interface . . .

        #endregion Class Interface . . .

        #region Instance Interface . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>(An Action that handles HTTP GET requests) gets version information.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <returns>The version information.</returns>
        ///-------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("/api/")]
        [Route("/api/version/")]
        public virtual IActionResult GetVersionInfo()
        {
            // **** create a basic tuple to return ****

            List<KeyValuePair<string, string>> information = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Application", AppDomain.CurrentDomain.FriendlyName),
                new KeyValuePair<string, string>("Version", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString()),
                new KeyValuePair<string, string>("Runtime", Environment.Version.ToString()),
            };

            // **** add the list of configuration keys and values ****

            IEnumerable<IConfigurationSection> configItems = _config.GetChildren();

            foreach (IConfigurationSection configItem in configItems)
            {
                information.Add(
                    new KeyValuePair<string, string>(
                        $"Config:{configItem.Key}",
                        configItem.Value
                        )
                    );
            }

            // **** try to get a list of routes ****

            try
            {
                List<(string Action, string Controller, string Name, string Template)> routes =
                    _provider.ActionDescriptors.Items.Select(x => (
                        Action: x.RouteValues["Action"],
                        Controller: x.RouteValues["Controller"],
                        Name: x.AttributeRouteInfo.Name,
                        Template: x.AttributeRouteInfo.Template
                    )).ToList();

                // *** add to our return list ****

                information.Add(
                    new KeyValuePair<string, string>(
                        "Routes",
                        JsonConvert.SerializeObject(routes)
                        )
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            // **** return our information ****

            return StatusCode(200, information);
        }

        #endregion Instance Interface . . .

        #region Internal Functions . . .

        #endregion Internal Functions . . .


    }
}
