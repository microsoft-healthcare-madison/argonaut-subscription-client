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

        public enum EndpointChannelType {
            RestHook,
            Websocket,
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
        /// <summary>Gets or sets URL of the document.</summary>
        ///
        /// <value>The URL.</value>
        ///-------------------------------------------------------------------------------------------------

        public string Url { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the endpoint clients.</summary>
        ///
        /// <value>The endpoint clients.</value>
        ///-------------------------------------------------------------------------------------------------

        public Dictionary<Guid, Guid> EndpointClients { get; set; }

        #endregion Instance Variables . . .

        #region Constructors . . .

        #endregion Constructors . . .

        #region Class Interface . . .

        #endregion Class Interface . . .

        #region Instance Interface . . .

        #endregion Instance Interface . . .

        #region Internal Functions . . .

        #endregion Internal Functions . . .


    }
}
