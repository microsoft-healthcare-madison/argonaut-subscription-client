using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace argonaut_subscription_client_host.Models
{
    public class EndpointInformation
    {
        #region Class Enums . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Values that represent endpoint channel types.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        public enum EndpointChannelType: int {
            RestHook = 0,
            WebSocket,
            ServerSideEvent
        }

        #endregion Class Enums . . .

        #region Class Variables . . .

        #endregion Class Variables . . .

        #region Instance Variables . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the unique id for this endpoint</summary>
        ///
        /// <value>The UID.</value>
        ///-------------------------------------------------------------------------------------------------

        public Guid Uid { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the type of the channel.</summary>
        ///
        /// <value>The type of the channel.</value>
        ///-------------------------------------------------------------------------------------------------

        public EndpointChannelType ChannelType { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets URL part for this endpoint.</summary>
        ///
        /// <value>The URL.</value>
        ///-------------------------------------------------------------------------------------------------

        public string UrlPart { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets a value indicating whether this endpoint is enabled.</summary>
        ///
        /// <value>True if enabled, false if not.</value>
        ///-------------------------------------------------------------------------------------------------

        public bool Enabled { get; set; }

        /////-------------------------------------------------------------------------------------------------
        ///// <summary>Gets or sets the endpoint clients.</summary>
        /////
        ///// <value>The endpoint clients.</value>
        /////-------------------------------------------------------------------------------------------------

        //[JsonIgnoreAttribute]
        //public Dictionary<Guid, Guid> EndpointClients { get; set; }

        #endregion Instance Variables . . .

        #region Constructors . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Creates a new EndpointInformation.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="uid">        The UID.</param>
        /// <param name="channelType">The type of the channel.</param>
        /// <param name="urlPart">    The URL.</param>
        ///
        /// <returns>An EndpointInformation.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static EndpointInformation Create(Guid uid, EndpointChannelType channelType, string urlPart)
        {
            return new EndpointInformation()
            {
                Uid = uid,
                ChannelType = channelType,
                UrlPart = urlPart,
                Enabled = true,
                //EndpointClients = new Dictionary<Guid, Guid>()
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
