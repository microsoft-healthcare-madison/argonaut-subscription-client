using argonaut_subscription_client_host.Managers;
using argonaut_subscription_client_host.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace argonaut_subscription_client_host.Controllers
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>A controller for handling Subscription Endpoints.
    /// Responds to:
    ///     GET:    /api/Clients/{clientUid}/Endpoints/
    ///     POST:   /api/Clients/{clientUid}/Endpoints/
    ///     POST:   /api/Clients/{clientUid}/Endpoints/{endpointUid}/
    ///     DELETE: /api/Clients/{clientUid}/Endpoints/{endpointUid}/
    /// </summary>
    ///
    /// <remarks>Gino Canessa, 7/26/2019.</remarks>
    ///-------------------------------------------------------------------------------------------------

    [Produces("application/json")]
    public class ClientEndpointController : Controller
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

        static ClientEndpointController()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Constructor.</summary>
        ///
        /// <remarks>Gino Canessa, 7/26/2019.</remarks>
        ///
        /// <param name="iConfiguration">Reference to the injected configuration object</param>
        ///-------------------------------------------------------------------------------------------------

        public ClientEndpointController(
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
        /// <summary>(An Action that handles HTTP GET requests) gets list client endpoints.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="clientUid">The client UID.</param>
        ///
        /// <returns>The list client endpoints.</returns>
        ///-------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("/api/Clients/{clientUid:guid}/Endpoints/")]
        public virtual IActionResult GetListClientEndpoints([FromRoute] Guid clientUid)
        {
            // **** return our list of endpoints ****

            return StatusCode(200, EndpointManager.GetEndpointListForClient(clientUid));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>(An Action that handles HTTP POST requests) posts a create REST endpoint for 
        /// a specified client.</summary>
        ///
        /// <remarks>Gino Canessa, 8/15/2019.</remarks>
        ///
        /// <param name="clientUid">The client UID.</param>
        ///
        /// <returns>An IActionResult.</returns>
        ///-------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("/api/Clients/{clientUid:guid}/Endpoints/REST/")]
        public virtual IActionResult PostCreateRestEndpointForClient([FromRoute] Guid clientUid)
        {
            // **** check for no information ****

            if ((clientUid == null) || (clientUid == Guid.Empty))
            {
                // **** fail ****

                return StatusCode(400);
            }

            // **** create a new endpoint object ****

            EndpointInformation endpoint = new EndpointInformation()
            {
                Uid = Guid.NewGuid(),
                ChannelType = EndpointInformation.EndpointChannelType.RestHook,
                Enabled = true,
                UrlPart = null
            };

            // **** create this endpoint and register to this client ****

            EndpointManager.AddOrUpdate(
                endpoint.Uid,
                endpoint.ChannelType,
                endpoint.UrlPart,
                clientUid
                );

            // **** return our object ****

            return StatusCode(201, endpoint);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>(An Action that handles HTTP POST requests) posts a create endpoint for client.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="clientUid">The client UID.</param>
        /// <param name="endpoint"> The endpoint.</param>
        ///
        /// <returns>An IActionResult.</returns>
        ///-------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("/api/Clients/{clientUid:guid}/Endpoints/")]
        public virtual IActionResult PostCreateEndpointForClient(
                                                                [FromRoute] Guid clientUid,
                                                                [FromBody] EndpointInformation endpoint
                                                                )
        {
            // **** check for no information ****

            if ((endpoint == null) || (clientUid == null) || (clientUid == Guid.Empty))
            {
                // **** fail ****

                return StatusCode(400);
            }

            // **** check for this URL part being available ****

            if (!EndpointManager.IsUrlPartAvailable(endpoint.UrlPart))
            {
                // **** fail ****

                return StatusCode(400, "{\"error\":\"URL Part already in use\"}");
            }

            // **** check for a UID ****

            if ((endpoint.Uid == null) ||
                (endpoint.Uid == Guid.Empty))
            {
                // **** add a guid ****

                endpoint.Uid = Guid.NewGuid();
            }

            // **** create this endpoint and register to this client ****

            EndpointManager.AddOrUpdate(
                endpoint.Uid,
                endpoint.ChannelType,
                endpoint.UrlPart,
                clientUid
                );

            // **** return our object ****

            return StatusCode(201, endpoint);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>(An Action that handles HTTP POST requests) posts a register endpoint for client.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">The endpoint UID.</param>
        ///
        /// <returns>An IActionResult.</returns>
        ///-------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("/api/Clients/{clientUid:guid}/Endpoints/{endpointUid:guid}/")]
        public virtual IActionResult PostRegisterEndpointForClient(
                                                                  [FromRoute] Guid clientUid,
                                                                  [FromRoute] Guid endpointUid
                                                                  )
        {
            // **** sanity checks ****

            if ((clientUid == null) || (clientUid == Guid.Empty) ||
                (endpointUid == null) || (endpointUid == Guid.Empty))
            {
                // **** fail ****

                return StatusCode(400);
            }

            // **** register this client to this endpoint ****

            EndpointManager.RegisterClientForEndpoint(clientUid, endpointUid);

            // **** return success ****

            return StatusCode(201);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>(An Action that handles HTTP POST requests) posts an endpoint operation for
        /// client.</summary>
        ///
        /// <remarks>Gino Canessa, 9/6/2019.</remarks>
        ///
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">The endpoint UID.</param>
        /// <param name="operation">  The operation.</param>
        ///
        /// <returns>An IActionResult.</returns>
        ///-------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("/api/Clients/{clientUid:guid}/Endpoints/{endpointUid:guid}/{operation}/")]
        public virtual IActionResult PostEndpointOperationForClient(
                                                                    [FromRoute] Guid clientUid,
                                                                    [FromRoute] Guid endpointUid,
                                                                    [FromRoute] string operation
                                                                    )
        {
            // **** sanity checks ****

            if ((clientUid == null) || (clientUid == Guid.Empty) ||
                (endpointUid == null) || (endpointUid == Guid.Empty) ||
                (string.IsNullOrEmpty(operation)))
            {
                // **** fail ****

                return StatusCode(400);
            }

            // **** convert the operation to lower-case for sanity in matching ****

            operation = operation.ToLower();

            // **** act depending on operation ****

            switch (operation)
            {
                case "enable":
                    {   // scope
                        if (EndpointManager.TryEnable(endpointUid, out EndpointInformation endpoint))
                        {
                            return StatusCode(200, endpoint);
                        }
                    }
                    break;

                case "disable":
                    {   // scope
                        if (EndpointManager.TryDisable(endpointUid, out EndpointInformation endpoint))
                        {
                            return StatusCode(200, endpoint);
                        }
                    }
                    break;
            }

            // **** failure, unknown operation ****

            return StatusCode(400);
        }


        [HttpDelete]
        [Route("/api/Clients/{clientUid:guid}/Endpoints/{endpointUid:guid}/")]
        public virtual IActionResult DeleteEndpointForClient(
                                                            [FromRoute] Guid clientUid,
                                                            [FromRoute] Guid endpointUid
                                                            )
        {
            // **** sanity checks ****

            if ((clientUid == null) || (clientUid == Guid.Empty) ||
                (endpointUid == null) || (endpointUid == Guid.Empty))
            {
                // **** fail ****

                return StatusCode(400);
            }

            // **** remove this endpoint ****

            EndpointManager.Remove(clientUid, endpointUid);

            // **** success ****

            return StatusCode(204);
        }

        #endregion Instance Interface . . .

        #region Internal Functions . . .

        #endregion Internal Functions . . .

    }
}
