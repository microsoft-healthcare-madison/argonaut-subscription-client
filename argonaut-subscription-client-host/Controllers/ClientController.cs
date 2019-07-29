using argonaut_subscription_client_host.Managers;
using argonaut_subscription_client_host.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace argonaut_subscription_client_host.Controllers
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>A controller for handling client registrations.
    /// Responds to:
    ///     GET:    /api/Clients/
    ///     POST:   /api/Clients/
    ///     DELETE: /api/Clients/{clientUid}/
    /// </summary>
    ///
    /// <remarks>Gino Canessa, 7/18/2019.</remarks>
    ///-------------------------------------------------------------------------------------------------

    [Produces("application/json")]
    public class ClientController : Controller
    {
        #region Class Variables . . .

        #endregion Class Variables . . .

        #region Instance Variables . . .

        /// <summary>   The configuration. </summary>
        private readonly IConfiguration _config;

        #endregion Instance Variables . . .

        #region Constructors . . .

        static ClientController()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Constructor.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <param name="iConfiguration">Reference to the injected configuration object</param>
        ///-------------------------------------------------------------------------------------------------

        public ClientController(
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
        /// <summary>(An Action that handles HTTP GET requests) gets list client registrations.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <returns>The list client registrations.</returns>
        ///-------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("/api/Clients/")]
        public virtual IActionResult GetListClients()
        {
            // **** return the list of client registrations ****

            return StatusCode(200, ClientManager.GetClientList());
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>(An Action that handles HTTP POST requests) posts a create client registration.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <param name="client">The client.</param>
        ///
        /// <returns>An IActionResult.</returns>
        ///-------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("/api/Clients/")]
        public virtual IActionResult PostCreateClient(
                                                    [FromBody] ClientInformation client
                                                    )
        {
            // **** check for no information ****

            if (client == null)
            {
                // **** fail ****

                return StatusCode(400);
            }

            // **** check for a UID ****

            if ((client.Uid == null) ||
                (client.Uid == Guid.Empty))
            {
                // **** add a guid ****

                client.Uid = Guid.NewGuid();
            }

            // **** add to our manager ****

            ClientManager.AddOrUpdate((Guid)client.Uid, client.FhirServerUrl);

            // **** return our data (so the client has the UID) ****

            return StatusCode(201, client);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>(An Action that handles HTTP DELETE requests) deletes the client registration
        /// described by clientUid.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <param name="clientUid">The client UID.</param>
        ///
        /// <returns>An IActionResult.</returns>
        ///-------------------------------------------------------------------------------------------------

        [HttpDelete]
        [Route("/api/Clients/{clientUid:guid}/")]
        public virtual IActionResult DeleteClient(
                                                [FromRoute] Guid clientUid
                                                )
        {
            // **** sanity checks ****

            if ((clientUid == null) || (clientUid == Guid.Empty))
            {
                // **** fail ****

                return StatusCode(400);
            }

            // **** remove the specified client ****

            if (!ClientManager.Remove(clientUid))
            {
                // **** fail ****

                return StatusCode(400);
            }

            // **** success ****

            return StatusCode(204);
        }

        #endregion Instance Interface . . .

        #region Internal Functions . . .

        #endregion Internal Functions . . .
    }
}
