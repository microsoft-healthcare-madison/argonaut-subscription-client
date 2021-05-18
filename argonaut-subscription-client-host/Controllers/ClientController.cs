// <copyright file="ClientController.cs" company="Microsoft Corporation">
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

namespace argonaut_subscription_client_host.Controllers
{
    /// <summary>
    /// A controller for handling client registrations. Responds to:
    ///     GET:    /api/Clients/
    ///     POST:   /api/Clients/
    ///     DELETE: /api/Clients/{clientUid}/
    /// </summary>
    [Produces("application/json")]
    public class ClientController : Controller
    {
        /// <summary>   The configuration. </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientController"/> class.
        /// </summary>
        /// <param name="iConfiguration">Zero-based index of the configuration.</param>
        public ClientController(IConfiguration iConfiguration)
        {
            _config = iConfiguration;
        }

        /// <summary>(An Action that handles HTTP GET requests) gets list client registrations.</summary>
        /// <returns>The list client registrations.</returns>
        [HttpGet]
        [Route("/api/Clients/")]
        public virtual IActionResult GetListClients()
        {
            return StatusCode(200, ClientManager.GetClientList());
        }

        /// <summary>
        /// (An Action that handles HTTP POST requests) posts a create client registration.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>An IActionResult.</returns>
        [HttpPost]
        [Route("/api/Clients/")]
        public virtual IActionResult PostCreateClient([FromBody] RequestedClientInformation requestedClient)
        {
            if (requestedClient == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest);
            }

            Guid clientGuid;

            if (!Guid.TryParse(requestedClient.Uid, out clientGuid))
            {
                clientGuid = Guid.NewGuid();
            }

            ClientInformation client = new ClientInformation()
            {
                FhirServerUrl = requestedClient.FhirServerUrl,
                Uid = clientGuid,
            };

            //// check for a UID
            //if ((client.Uid == null) ||
            //    (client.Uid == Guid.Empty))
            //{
            //    // add a guid
            //    client.Uid = Guid.NewGuid();
            //}

            ClientManager.AddOrUpdate((Guid)client.Uid, client.FhirServerUrl);

            // return our data (so the client has the UID)
            return StatusCode((int)HttpStatusCode.Created, client);
        }

        /// <summary>
        /// (An Action that handles HTTP DELETE requests) deletes the client registration described by
        /// clientUid.
        /// </summary>
        /// <param name="clientUid">The client UID.</param>
        /// <returns>An IActionResult.</returns>
        [HttpDelete]
        [Route("/api/Clients/{clientUid:guid}/")]
        public virtual IActionResult DeleteClient([FromRoute] Guid clientUid)
        {
            if (clientUid == Guid.Empty)
            {
                return StatusCode((int)HttpStatusCode.BadRequest);
            }

            // remove the specified client
            if (!ClientManager.Remove(clientUid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest);
            }

            return StatusCode((int)HttpStatusCode.NoContent);
        }
    }
}
