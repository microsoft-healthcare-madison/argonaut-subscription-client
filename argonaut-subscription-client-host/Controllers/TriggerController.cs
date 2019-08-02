using argonaut_subscription_client_host.Managers;
using argonaut_subscription_client_host.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace argonaut_subscription_client_host.Controllers
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>A controller for handling trigger requests.
    /// Responds to:
    ///     GET:    /Triggers/
    ///     POST:   /Triggers/
    ///     GET:    /Triggers/{triggerUid}/
    ///     DELETE: /Triggers/{triggerUid}/
    ///     GET:    /Triggers/Resources/
    /// 
    /// </summary>
    ///
    /// <remarks>Gino Canessa, 7/31/2019.</remarks>
    ///-------------------------------------------------------------------------------------------------

    [Produces("application/json")]
    public class TriggerController : Controller
    {
        #region Class Variables . . .

        #endregion Class Variables . . .

        #region Instance Variables . . .

        /// <summary>   The configuration. </summary>
        private readonly IConfiguration _config;

        #endregion Instance Variables . . .

        #region Constructors . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Static constructor.</summary>
        ///
        /// <remarks>Gino Canessa, 7/31/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        static TriggerController()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Constructor.</summary>
        ///
        /// <remarks>Gino Canessa, 7/31/2019.</remarks>
        ///
        /// <param name="iConfiguration">Zero-based index of the configuration.</param>
        ///-------------------------------------------------------------------------------------------------

        public TriggerController(
                                IConfiguration iConfiguration
                                )
        {
            // **** grab a reference to our application configuration ****

            _config = iConfiguration;
        }

        #endregion Constructors . . .

        #region Class Interface . . .

        #endregion Class Interface . . .

        #region Instance Interface . . .

        [HttpPost]
        [Route("/Triggers/")]
        public virtual IActionResult PostTriggerRequest([FromBody] TriggerRequest triggerRequest)
        {
            // **** sanity checks ****

            if ((triggerRequest == null) ||
                (string.IsNullOrEmpty(triggerRequest.FhirServerUrl)) ||
                (string.IsNullOrEmpty(triggerRequest.ResourceName)))
            {
                // **** cannot process, bad request ****

                return StatusCode(400);
            }

            // **** post to the processor ****

            if (TriggerManager.TryAddRequest(triggerRequest, out TriggerInformation triggerInfo))
            {
                return StatusCode(202, triggerInfo);
            }

            // **** failure ****

            return StatusCode(500);
        }

        #endregion Instance Interface . . .

        #region Internal Functions . . .

        #endregion Internal Functions . . .

    }
}
