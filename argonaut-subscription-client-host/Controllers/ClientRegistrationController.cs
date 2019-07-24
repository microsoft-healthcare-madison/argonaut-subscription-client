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
    ///     GET:    /api/ClientRegistration/
    ///     POST:   /api/ClientRegistration/
    ///     DELETE: /api/ClientRegistration/{clientUid}/
    /// </summary>
    ///
    /// <remarks>Gino Canessa, 7/18/2019.</remarks>
    ///-------------------------------------------------------------------------------------------------

    [Produces("application/json")]
    public class ClientRegistrationController : Controller
    {
        #region Class Variables . . .

        #endregion Class Variables . . .

        #region Instance Variables . . .

        /// <summary>   The configuration. </summary>
        private readonly IConfiguration _config;

        #endregion Instance Variables . . .

        #region Constructors . . .

        static ClientRegistrationController()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Constructor.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <param name="iConfiguration">Reference to the injected configuration object</param>
        ///-------------------------------------------------------------------------------------------------

        public ClientRegistrationController(
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
        [Route("/api/ClientRegistration")]
        public virtual IActionResult GetListClientRegistrations()
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
        [Route("/api/ClientRegistration")]
        public virtual IActionResult PostCreateClientRegistration(
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

            ClientManager.AddOrUpdate(client);

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
        [Route("/api/ClientRegistration/{clientUid}/")]
        public virtual IActionResult DeleteClientRegistration(
                                                             [FromRoute] string clientUid
                                                             )
        {
            // **** sanity checks ****

            if ((string.IsNullOrEmpty(clientUid)) ||
                (!Guid.TryParse(clientUid, out Guid clientGuid)))
            {
                // **** fail ****

                return StatusCode(400);
            }

            // **** remove the specified client ****

            if (!ClientManager.Remove(clientGuid))
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
