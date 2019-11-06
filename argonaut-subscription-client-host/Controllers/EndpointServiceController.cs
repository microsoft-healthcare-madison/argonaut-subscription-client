using argonaut_subscription_client_host.Managers;
using argonaut_subscription_client_host.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace argonaut_subscription_client_host.Controllers
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>A controller for handling subcription notifications (Endpoint responding)
    /// Responds to:
    ///     GET:    /Endpoints/
    ///     GET:    /Endpoints/{endpointUid}/
    ///     GET:    /Endpoints/{endpointName}/
    ///     POST:   /Endpoints/{endpointUid}/
    ///     POST:   /Endpoints/{endpointName}/
    /// </summary>
    ///
    /// <remarks>Gino Canessa, 7/26/2019.</remarks>
    ///-------------------------------------------------------------------------------------------------
    [Produces("application/json")]

    public class EndpointServiceController : Controller
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
        /// <remarks>Gino Canessa, 7/26/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        static EndpointServiceController()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Constructor.</summary>
        ///
        /// <remarks>Gino Canessa, 7/26/2019.</remarks>
        ///
        /// <param name="iConfiguration">Reference to the injected configuration object</param>
        ///-------------------------------------------------------------------------------------------------

        public EndpointServiceController(
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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>(An Action that handles HTTP GET requests) gets list endpoints.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <returns>The list endpoints.</returns>
        ///-------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("/Endpoints/")]
        public virtual IActionResult GetListEndpoints()
        {
            // **** return the list of endpoints ****

            return StatusCode(200, EndpointManager.GetEndpointList());
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>(An Action that handles HTTP GET requests) gets endpoint information by UID.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="endpointUid">The endpoint UID.</param>
        ///
        /// <returns>The endpoint information by UID.</returns>
        ///-------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("/Endpoints/{endpointUid:guid}/")]
        public virtual IActionResult GetEndpointInfoByUid([FromRoute] Guid endpointUid)
        {
            // **** if found, return this Endpoint's information ****

            if (EndpointManager.TryGetEndpointByUid(endpointUid, out EndpointInformation endpoint))
            {
                return StatusCode(200, endpoint);
            }

            // **** return not found ****

            return StatusCode(404);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>(An Action that handles HTTP GET requests) gets endpoint information by URL.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="urlPart">The URL part.</param>
        ///
        /// <returns>The endpoint information by URL.</returns>
        ///-------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("/Endpoints/{urlPart}/")]
        public virtual IActionResult GetEndpointInfoByUrl([FromRoute] string urlPart)
        {
            // **** if found, return this Endpoint's information ****

            if (EndpointManager.TryGetEndpointByUrlPart(urlPart, out EndpointInformation endpoint))
            {
                return StatusCode(200, endpoint);
            }

            // **** return not found ****

            return StatusCode(404);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>(An Action that handles HTTP POST requests) posts an event to endpoint by UID.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="endpointUid">The endpoint UID.</param>
        /// <param name="content">    The content.</param>
        ///
        /// <returns>An IActionResult.</returns>
        ///-------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("/Endpoints/{endpointUid:guid}/")]
        [Consumes("application/fhir+json", new[] { "application/json" })]
        public virtual IActionResult PostEventToEndpointByUid([FromRoute] Guid endpointUid)
        {
            // **** check to see if this endpoint exists ****

            if (!EndpointManager.TryGetEndpointByUid(endpointUid, out EndpointInformation endpoint))
            {
                // **** notify user ****

                Console.WriteLine($"Received message for unknown Endpoint: {endpointUid}");

                // **** allow it, but note we are not processing it ****

                return StatusCode(202);
            }

            // **** check for disabled ****

            if (endpoint.Enabled == false)
            {
                // **** notify user ****

                Console.WriteLine($"Received message for DISABLED Endpoint: {endpointUid}, refusing!");

                // **** reject ****

                return StatusCode(500);
            }

            // **** notify user ****

            Console.WriteLine($"Received message for Endpoint: {endpointUid}");

            // **** read our content ****

            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string content = reader.ReadToEnd();

                // **** pass this to the endpoint manager for processing ****

                EndpointManager.QueueMessage(endpointUid, content);
            }

            // **** flag we accepted ****

            return StatusCode(204);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>(An Action that handles HTTP POST requests) posts an event to endpoint by URL
        /// part.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="urlPart">The URL part.</param>
        /// <param name="content">The content.</param>
        ///
        /// <returns>An IActionResult.</returns>
        ///-------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("/Endpoints/{urlPart}/")]
        [Consumes("application/fhir+json", new[] { "application/json" })]
        public virtual IActionResult PostEventToEndpointByUrlPart([FromRoute] string urlPart)
        {
            // **** check to see if this endpoint exists ****

            if (!EndpointManager.Exists(urlPart))
            {
                // **** notify user ****

                Console.WriteLine($"Received message for unknown Endpoint Url: {urlPart}");

                // **** allow it, but note we are not processing it ****

                return StatusCode(202);
            }

            // **** notify user ****

            Console.WriteLine($"Received message for Endpoint: {urlPart}");

            // **** read our content ****

            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string content = reader.ReadToEnd();

                // **** pass this to the endpoint manager for processing ****

                EndpointManager.QueueMessage(urlPart, content);
            }

            // **** flag we accepted ****

            return StatusCode(204);
        }

        #endregion Instance Interface . . .

        #region Internal Functions . . .

        #endregion Internal Functions . . .

    }
}
