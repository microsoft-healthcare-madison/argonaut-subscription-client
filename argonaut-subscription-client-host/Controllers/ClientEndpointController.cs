// <copyright file="ClientEndpointController.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using argonaut_subscription_client_host.Managers;
using argonaut_subscription_client_host.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace argonaut_subscription_client_host.Controllers
{
    /// <summary>A controller for handling Subscription Endpoints.
    /// Responds to:
    ///     GET:    /api/Clients/{clientUid}/Endpoints/
    ///     POST:   /api/Clients/{clientUid}/Endpoints/
    ///     POST:   /api/Clients/{clientUid}/Endpoints/{endpointUid}/
    ///     DELETE: /api/Clients/{clientUid}/Endpoints/{endpointUid}/
    /// </summary>
    [Produces("application/json")]
    public class ClientEndpointController : Controller
    {
        /// <summary>   The configuration. </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientEndpointController"/> class.
        /// </summary>
        /// <param name="iConfiguration">Zero-based index of the configuration.</param>
        public ClientEndpointController(IConfiguration iConfiguration)
        {
            _config = iConfiguration;
        }

        /// <summary>(An Action that handles HTTP GET requests) gets list client endpoints.</summary>
        /// <param name="clientUid">The client UID.</param>
        /// <returns>The list client endpoints.</returns>
        [HttpGet]
        [Route("/api/Clients/{clientUid:guid}/Endpoints/")]
        public virtual IActionResult GetListClientEndpoints([FromRoute] Guid clientUid)
        {
            return StatusCode((int)HttpStatusCode.OK, EndpointManager.GetEndpointListForClient(clientUid));
        }

        /// <summary>
        /// (An Action that handles HTTP POST requests) posts a create REST endpoint for a specified
        /// client.
        /// </summary>
        /// <param name="clientUid">The client UID.</param>
        /// <returns>An IActionResult.</returns>
        [HttpPost]
        [Route("/api/Clients/{clientUid:guid}/Endpoints/REST/")]
        public virtual IActionResult PostCreateRestEndpointForClient([FromRoute] Guid clientUid)
        {
            if (clientUid == Guid.Empty)
            {
                return StatusCode((int)HttpStatusCode.BadRequest);
            }

            EndpointInformation endpoint = new EndpointInformation(
                Guid.NewGuid(), 
                EndpointInformation.EndpointChannelType.RestHook, 
                null);

            EndpointManager.AddOrUpdate(
                endpoint.Uid,
                endpoint.ChannelType,
                endpoint.UrlPart,
                clientUid);

            return StatusCode((int)HttpStatusCode.Created, endpoint);
        }

        /// <summary>(An Action that handles HTTP POST requests) posts a create endpoint for client.</summary>
        /// <param name="clientUid">The client UID.</param>
        /// <param name="endpoint"> The endpoint.</param>
        /// <returns>An IActionResult.</returns>
        [HttpPost]
        [Route("/api/Clients/{clientUid:guid}/Endpoints/")]
        public virtual IActionResult PostCreateEndpointForClient(
            [FromRoute] Guid clientUid,
            [FromBody] EndpointInformation endpoint)
        {
            if (clientUid == Guid.Empty)
            {
                return StatusCode((int)HttpStatusCode.BadRequest);
            }

            if (!EndpointManager.IsUrlPartAvailable(endpoint.UrlPart))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "{\"error\":\"URL Part already in use\"}");
            }

            if (endpoint.Uid == Guid.Empty)
            {
                endpoint.Uid = Guid.NewGuid();
            }

            // create this endpoint and register to this client
            EndpointManager.AddOrUpdate(
                endpoint.Uid,
                endpoint.ChannelType,
                endpoint.UrlPart,
                clientUid);

            return StatusCode((int)HttpStatusCode.Created, endpoint);
        }
    
        /// <summary>(An Action that handles HTTP POST requests) posts a register endpoint for client.</summary>
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">The endpoint UID.</param>
        /// <returns>An IActionResult.</returns>
        [HttpPost]
        [Route("/api/Clients/{clientUid:guid}/Endpoints/{endpointUid:guid}/")]
        public virtual IActionResult PostRegisterEndpointForClient(
            [FromRoute] Guid clientUid,
            [FromRoute] Guid endpointUid)
        {
            if ((clientUid == Guid.Empty) || (endpointUid == Guid.Empty))
            {
                return StatusCode((int)HttpStatusCode.BadRequest);
            }

            // register this client to this endpoint
            EndpointManager.RegisterClientForEndpoint(clientUid, endpointUid);

            return StatusCode((int)HttpStatusCode.Created);
        }

    
        /// <summary>(An Action that handles HTTP POST requests) posts an endpoint operation for
        /// client.</summary>
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">The endpoint UID.</param>
        /// <param name="operation">  The operation.</param>
        /// <returns>An IActionResult.</returns>
        [HttpPost]
        [Route("/api/Clients/{clientUid:guid}/Endpoints/{endpointUid:guid}/{operation}/")]
        public virtual IActionResult PostEndpointOperationForClient(
            [FromRoute] Guid clientUid,
            [FromRoute] Guid endpointUid,
            [FromRoute] string operation)
        {
            if ((clientUid == Guid.Empty) || 
                (endpointUid == Guid.Empty) ||
                (string.IsNullOrEmpty(operation)))
            {
                return StatusCode((int)HttpStatusCode.BadRequest);
            }

            operation = operation.ToLower();

            switch (operation)
            {
                case "enable":
                    if (EndpointManager.TryEnable(endpointUid, out EndpointInformation enabledEndpoint))
                    {
                        return StatusCode((int)HttpStatusCode.OK, enabledEndpoint);
                    }
                    break;

                case "disable":
                    if (EndpointManager.TryDisable(endpointUid, out EndpointInformation disabledEndpoint))
                    {
                        return StatusCode((int)HttpStatusCode.OK, disabledEndpoint);
                    }
                    break;
            }

            // unknown operation
            return StatusCode((int)HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// (An Action that handles HTTP DELETE requests) deletes the endpoint for client.
        /// </summary>
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">The endpoint UID.</param>
        /// <returns>An IActionResult.</returns>
        [HttpDelete]
        [Route("/api/Clients/{clientUid:guid}/Endpoints/{endpointUid:guid}/")]
        public virtual IActionResult DeleteEndpointForClient(
            [FromRoute] Guid clientUid,
            [FromRoute] Guid endpointUid)
        {
            if ((clientUid == Guid.Empty) || (endpointUid == Guid.Empty))
            {
                return StatusCode((int)HttpStatusCode.BadRequest);
            }

            EndpointManager.Remove(clientUid, endpointUid);

            return StatusCode((int)HttpStatusCode.NoContent);
        }
    }
}
