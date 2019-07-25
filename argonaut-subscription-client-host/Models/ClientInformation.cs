using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace argonaut_subscription_client_host.Models
{
    public class ClientInformation
    {
        #region Class Variables . . .

        #endregion Class Variables . . .

        #region Instance Variables . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets a unique identifier of the client.</summary>
        ///
        /// <value>Unique identifier of the client.</value>
        ///-------------------------------------------------------------------------------------------------

        public Guid? Uid { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets URL of the FHIR server.</summary>
        ///
        /// <value>The fhir server URL.</value>
        ///-------------------------------------------------------------------------------------------------

        public string FhirServerUrl { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the endpoints.</summary>
        ///
        /// <value>The endpoints.</value>
        ///-------------------------------------------------------------------------------------------------

        [JsonIgnoreAttribute]
        public Dictionary<Guid, Guid> ClientEndpoints { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the message queue for this client</summary>
        ///
        /// <value>The message queue.</value>
        ///-------------------------------------------------------------------------------------------------

        [JsonIgnoreAttribute]
        public ConcurrentQueue<string> MessageQ { get; set; }

        #endregion Instance Variables . . .

        #region Constructors . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Creates a new ClientInformation.</summary>
        ///
        /// <remarks>Gino Canessa, 7/25/2019.</remarks>
        ///
        /// <param name="uid">          Unique identifier of the client.</param>
        /// <param name="fhirServerUrl">The fhir server URL.</param>
        ///
        /// <returns>A ClientInformation.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static ClientInformation Create(Guid uid, string fhirServerUrl)
        {
            return new ClientInformation()
            {
                Uid = uid,
                FhirServerUrl = fhirServerUrl,
                ClientEndpoints = new Dictionary<Guid, Guid>(),
                MessageQ = new ConcurrentQueue<string>(),
            };
        }

        #endregion Constructors . . .

        #region Class Interface . . .

        #endregion Class Interface . . .

        #region Instance Interface . . .

        #endregion Instance Interface . . .

        #region Internal Functions . . .

        #endregion Internal Functions . . .

    }
}
