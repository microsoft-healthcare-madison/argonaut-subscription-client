// <copyright file="EndpointServiceController.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using argonaut_subscription_client_host.Managers;
using argonaut_subscription_client_host.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace argonaut_subscription_client_host.Controllers
{
    /// <summary>A controller for handling subcription notifications (Endpoint responding)
    /// Responds to:
    ///     GET:    /Endpoints/
    ///     GET:    /Endpoints/{endpointUid}/
    ///     GET:    /Endpoints/{endpointName}/
    ///     POST:   /Endpoints/{endpointUid}/
    ///     POST:   /Endpoints/{endpointName}/
    /// </summary>
    [Produces("application/json")]

    public class EndpointServiceController : Controller
    {
        /// <summary>   The configuration. </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointServiceController"/> class.
        /// </summary>
        /// <param name="iConfiguration">Zero-based index of the configuration.</param>
        public EndpointServiceController(IConfiguration iConfiguration )
        {
            _config = iConfiguration;
        }
    
        /// <summary>(An Action that handles HTTP GET requests) gets list endpoints.</summary>
        /// <returns>The list endpoints.</returns>
        [HttpGet]
        [Route("/Endpoints/")]
        public virtual IActionResult GetListEndpoints()
        {
            return StatusCode((int)HttpStatusCode.OK, EndpointManager.GetEndpointList());
        }

        /// <summary>(An Action that handles HTTP GET requests) gets endpoint information by UID.</summary>
        /// <param name="endpointUid">The endpoint UID.</param>
        /// <returns>The endpoint information by UID.</returns>
        [HttpGet]
        [Route("/Endpoints/{endpointUid:guid}/")]
        public virtual IActionResult GetEndpointInfoByUid([FromRoute] Guid endpointUid)
        {
            if (EndpointManager.TryGetEndpointByUid(endpointUid, out EndpointInformation endpoint))
            {
                return StatusCode((int)HttpStatusCode.OK, endpoint);
            }

            return StatusCode((int)HttpStatusCode.NotFound);
        }

        /// <summary>(An Action that handles HTTP GET requests) gets endpoint information by URL.</summary>
        /// <param name="urlPart">The URL part.</param>
        /// <returns>The endpoint information by URL.</returns>
        [HttpGet]
        [Route("/Endpoints/{urlPart}/")]
        public virtual IActionResult GetEndpointInfoByUrl([FromRoute] string urlPart)
        {
            if (EndpointManager.TryGetEndpointByUrlPart(urlPart, out EndpointInformation endpoint))
            {
                return StatusCode((int)HttpStatusCode.OK, endpoint);
            }

            return StatusCode((int)HttpStatusCode.NotFound);
        }
    
        /// <summary>(An Action that handles HTTP POST requests) posts an event to endpoint by UID.</summary>
        /// <param name="endpointUid">The endpoint UID.</param>
        /// <param name="content">    The content.</param>
        /// <returns>An IActionResult.</returns>
        [HttpPost]
        [Route("/Endpoints/{endpointUid:guid}/")]
        [Consumes("application/fhir+json", new[] { "application/json" })]
        public virtual IActionResult PostEventToEndpointByUid([FromRoute] Guid endpointUid)
        {
            if (!EndpointManager.TryGetEndpointByUid(endpointUid, out EndpointInformation endpoint))
            {
                Console.WriteLine($"Received message for unknown Endpoint: {endpointUid}");

                // assume this endpoint has been removed
                return StatusCode((int)HttpStatusCode.Gone);
            }

            if (endpoint.Enabled == false)
            {
                Console.WriteLine($"Received message for DISABLED Endpoint: {endpointUid}, refusing!");

                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            Console.WriteLine($"Received message for Endpoint: {endpointUid}");

            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string content = reader.ReadToEndAsync().Result;

                EndpointManager.QueueMessage(endpointUid, content);
            }

            return StatusCode((int)HttpStatusCode.NoContent);
        }

    
        /// <summary>(An Action that handles HTTP POST requests) posts an event to endpoint by URL
        /// part.</summary>
        /// <param name="urlPart">The URL part.</param>
        /// <param name="content">The content.</param>
        /// <returns>An IActionResult.</returns>
        [HttpPost]
        [Route("/Endpoints/{urlPart}/")]
        [Consumes("application/fhir+json", new[] { "application/json" })]
        public virtual IActionResult PostEventToEndpointByUrlPart([FromRoute] string urlPart)
        {
            if (!EndpointManager.Exists(urlPart))
            {
                Console.WriteLine($"Received message for unknown Endpoint Url: {urlPart}");

                // assume this endpoint has been removed
                return StatusCode((int)HttpStatusCode.Gone);
            }

            Console.WriteLine($"Received message for Endpoint: {urlPart}");

            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string content = reader.ReadToEnd();

                EndpointManager.QueueMessage(urlPart, content);
            }

            return StatusCode((int)HttpStatusCode.NoContent);
        }
    }
}
