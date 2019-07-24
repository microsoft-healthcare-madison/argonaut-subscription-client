using argonaut_subscription_client_host.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace argonaut_subscription_client_host.Managers
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>Manager singleton for clients.</summary>
    ///
    /// <remarks>Gino Canessa, 7/18/2019.</remarks>
    ///-------------------------------------------------------------------------------------------------

    public class ClientManager
    {
        #region Class Variables . . .

        /// <summary>The instance for singleton pattern.</summary>
        private static ClientManager _instance;

        #endregion Class Variables . . .

        #region Instance Variables . . .

        /// <summary>Dictionary of clients, by uid.</summary>
        private Dictionary<Guid, ClientInformation> _uidClientDict;

        #endregion Instance Variables . . .

        #region Constructors . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Constructor that prevents any external instances of this class from being created.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        private ClientManager()
        {
            // **** create our index objects ****

            _uidClientDict = new Dictionary<Guid, ClientInformation>();
        }
        #endregion Constructors . . .

        #region Class Interface . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Initializes this object.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        public static void Init()
        {
            // **** make an instance ****

            CheckOrCreateInstance();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets client list.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <returns>The client list.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static List<ClientInformation> GetClientList()
        {
            // **** return our list of clients ****

            return _instance._uidClientDict.Values.ToList<ClientInformation>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Adds or updates a client record.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <param name="client">The client.</param>
        ///-------------------------------------------------------------------------------------------------

        public static void AddOrUpdate(ClientInformation client)
        {
            // **** add or update the record ****

            _instance._uidClientDict[client.Uid] = client;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Removes the given clientGuid.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <param name="clientGuid">The client Unique identifier to remove.</param>
        ///
        /// <returns>True if it succeeds, false if it fails.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static bool Remove(Guid clientGuid)
        {
            // **** check if this client exists ****

            if (!_instance._uidClientDict.ContainsKey(clientGuid))
            {
                return false;
            }

            // TODO(ginoc): handle cleaning up endpoints

            // **** remove this client ****

            _instance._uidClientDict.Remove(clientGuid);

            // **** successfully removed ****

            return true;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Determine if 'clientUid' exists.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <param name="clientUid">The client UID.</param>
        ///
        /// <returns>True if it succeeds, false if it fails.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static bool Exists(string clientUid)
        {
            // **** sanity checks ****

            if ((string.IsNullOrEmpty(clientUid)) ||
                (!Guid.TryParse(clientUid, out Guid clientGuid)))
            {
                // **** fail ****

                return false;
            }

            // **** check to see if we have this ****

            return _instance._uidClientDict.ContainsKey(clientGuid);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Attempts to parse a GUID from the given string.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <param name="clientUid"> The client UID.</param>
        /// <param name="clientGuid">[out] The client Unique identifier to remove.</param>
        ///
        /// <returns>True if it succeeds, false if it fails.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static bool TryParse(string clientUid, out Guid clientGuid)
        {
            clientGuid = Guid.Empty;

            // **** sanity checks ****

            if ((string.IsNullOrEmpty(clientUid)) ||
                (!Guid.TryParse(clientUid, out Guid parsedGuid)))
            {
                // **** fail ****

                return false;
            }

            // **** check to see if we have this ****

            if (_instance._uidClientDict.ContainsKey(parsedGuid))
            {
                clientGuid = parsedGuid;
                return true;
            }

            return false;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Attempts to get client a ClientInformation from the given GUID.</summary>
        ///
        /// <remarks>Gino Canessa, 7/19/2019.</remarks>
        ///
        /// <param name="clientGuid">The client Unique identifier to remove.</param>
        /// <param name="client">    [out] The client.</param>
        ///
        /// <returns>True if it succeeds, false if it fails.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static bool TryGetClient(Guid clientGuid, out ClientInformation client)
        {
            // **** check for this client existing ****

            if (_instance._uidClientDict.ContainsKey(clientGuid))
            {
                // **** set our client object ****

                client = _instance._uidClientDict[clientGuid];

                // **** success ****

                return true;
            }

            // **** not found ****

            client = null;

            // **** failure ****

            return false;
        }

        #endregion Class Interface . . .

        #region Instance Interface . . .

        #endregion Instance Interface . . .

        #region Internal Functions . . .


        ///-------------------------------------------------------------------------------------------------
        /// <summary>Check or create instance.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        private static void CheckOrCreateInstance()
        {
            if (_instance == null)
            {
                _instance = new ClientManager();
            }
        }
        #endregion Internal Functions . . .

    }
}

