using argonaut_subscription_client_host.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace argonaut_subscription_client_host.Managers
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>Manager singleton for endpoints.</summary>
    ///
    /// <remarks>Gino Canessa, 7/18/2019.</remarks>
    ///-------------------------------------------------------------------------------------------------

    public class EndpointManager
    {
        #region Class Variables . . .

        /// <summary>The instance for singleton pattern.</summary>
        private static EndpointManager _instance;

        #endregion Class Variables . . .

        #region Instance Variables . . .

        /// <summary>Dictionary of endpoints, by uid.</summary>
        private Dictionary<Guid, EndpointInformation> _uidEndpointDict;

        /// <summary>Dictionary of endpoints, by URL part.</summary>
        private Dictionary<string, EndpointInformation> _urlPartEndpointDict;

        /// <summary>Dictionary of clients per endpoint guid</summary>
        private Dictionary<Guid, HashSet<Guid>> _endpointClientsDict;

        /// <summary>Dictionary of endpoints per client guid</summary>
        private Dictionary<Guid, HashSet<Guid>> _clientEndpointsDict;

        #endregion Instance Variables . . .

        #region Constructors . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Constructor that prevents any external instance of this class from being created.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        private EndpointManager()
        {
            // **** create our index objects ****

            _uidEndpointDict = new Dictionary<Guid, EndpointInformation>();
            _urlPartEndpointDict = new Dictionary<string, EndpointInformation>();
            _endpointClientsDict = new Dictionary<Guid, HashSet<Guid>>();
            _clientEndpointsDict = new Dictionary<Guid, HashSet<Guid>>();
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
        /// <summary>Gets endpoint list.</summary>
        ///
        /// <remarks>Gino Canessa, 7/26/2019.</remarks>
        ///
        /// <returns>The endpoint list.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static List<EndpointInformation> GetEndpointList()
        {
            // **** return our list of endpoints ****

            return _instance._uidEndpointDict.Values.ToList<EndpointInformation>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets endpoint list for client.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="clientUid">The client UID.</param>
        ///
        /// <returns>The endpoint list for client.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static List<EndpointInformation> GetEndpointListForClient(Guid clientUid)
        {
            // **** pass to instance level ****

            return _instance._GetEndpointListForClient(clientUid);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Attempts to get endpoint by UID an EndpointInformation from the given GUID.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="endpointUid">The endpoint UID.</param>
        /// <param name="endpoint">   [out] The endpoint.</param>
        ///
        /// <returns>True if it succeeds, false if it fails.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static bool TryGetEndpointByUid(Guid endpointUid, out EndpointInformation endpoint)
        {
            // **** check for endpoint ****

            if (!_instance._uidEndpointDict.ContainsKey(endpointUid))
            {
                endpoint = null;
                return false;
            }

            // **** return endpoint ****

            endpoint = _instance._uidEndpointDict[endpointUid];
            return true;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Attempts to get endpoint by URL part an EndpointInformation from the given string.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="urlPart"> The URL part.</param>
        /// <param name="endpoint">[out] The endpoint.</param>
        ///
        /// <returns>True if it succeeds, false if it fails.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static bool TryGetEndpointByUrlPart(string urlPart, out EndpointInformation endpoint)
        {
            // **** check for endpoint ****

            if (!_instance._urlPartEndpointDict.ContainsKey(urlPart))
            {
                endpoint = null;
                return false;
            }

            // **** set endpoint info ****

            endpoint = _instance._urlPartEndpointDict[urlPart];
            return true;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Queries if an URL part is available.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="urlPart">The URL part.</param>
        ///
        /// <returns>True if the URL part is available, false if not.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static bool IsUrlPartAvailable(string urlPart)
        {
            // **** sanity check ****

            if (string.IsNullOrEmpty(urlPart))
            {
                return false;
            }

            // **** check to see if we have this url part ****

            return !_instance._urlPartEndpointDict.ContainsKey(urlPart);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Determine if 'endpointUid' exists.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="endpointUid">The endpoint UID.</param>
        ///
        /// <returns>True if it succeeds, false if it fails.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static bool Exists(Guid endpointUid)
        {
            return _instance._uidEndpointDict.ContainsKey(endpointUid);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Determine if 'urlPart' exists.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="urlPart">The URL part.</param>
        ///
        /// <returns>True if it succeeds, false if it fails.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static bool Exists(string urlPart)
        {
            return _instance._urlPartEndpointDict.ContainsKey(urlPart);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Adds an or update.</summary>
        ///
        /// <remarks>Gino Canessa, 7/26/2019.</remarks>
        ///
        /// <param name="endpointUid"> The endpoint UID.</param>
        /// <param name="endpointType">Type of the endpoint.</param>
        /// <param name="url">         URL of the resource.</param>
        /// <param name="clientUid">   (Optional) The client UID.</param>
        ///-------------------------------------------------------------------------------------------------

        public static void AddOrUpdate(Guid endpointUid, string endpointType, string url, Guid? clientUid = null)
        {
            // **** force to lower case for simplicity ****

            endpointType = endpointType.ToLower();

            // **** figure out the type ****

            if (endpointType.Contains("rest"))
            {
                _instance._AddOrUpdate(endpointUid, EndpointInformation.EndpointChannelType.RestHook, url, clientUid);
            }
            else if (endpointType.Contains("socket"))
            {
                _instance._AddOrUpdate(endpointUid, EndpointInformation.EndpointChannelType.WebSocket, url, clientUid);
            }
            else if (endpointType.Contains("event"))
            {
                _instance._AddOrUpdate(endpointUid, EndpointInformation.EndpointChannelType.ServerSideEvent, url, clientUid);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Adds an or update.</summary>
        ///
        /// <remarks>Gino Canessa, 7/26/2019.</remarks>
        ///
        /// <param name="endpointUid"> The endpoint UID.</param>
        /// <param name="endpointType">Type of the endpoint.</param>
        /// <param name="url">         URL of the resource.</param>
        /// <param name="clientUid">   (Optional) The client UID.</param>
        ///-------------------------------------------------------------------------------------------------

        public static void AddOrUpdate(
                                        Guid endpointUid, 
                                        EndpointInformation.EndpointChannelType endpointType, 
                                        string url,
                                        Guid? clientUid = null
                                        )
        {
            _instance._AddOrUpdate(endpointUid, endpointType, url, clientUid);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Registers a client to an endpoint.</summary>
        ///
        /// <remarks>Gino Canessa, 7/26/2019.</remarks>
        ///
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">The endpoint UID.</param>
        ///-------------------------------------------------------------------------------------------------

        public static void RegisterClientForEndpoint(Guid clientUid, Guid endpointUid)
        {
            _instance._RegisterClientForEndpoint(clientUid, endpointUid);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Unregister client, either from a specific endpoint or from all endpoints.</summary>
        ///
        /// <remarks>Gino Canessa, 7/26/2019.</remarks>
        ///
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">(Optional) The endpoint UID.</param>
        ///-------------------------------------------------------------------------------------------------

        public static void UnregisterClient(Guid clientUid, Guid? endpointUid = null)
        {
            _instance._UnregisterClient(clientUid, endpointUid);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Removes the specified endpoint from the specified client and cleans up orphaned endpoints
        /// </summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">The endpoint UID.</param>
        ///-------------------------------------------------------------------------------------------------

        public static void Remove(Guid clientUid, Guid endpointUid)
        {
            _instance._Remove(clientUid, endpointUid);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Removes the specified endpoint from all clients.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="endpointUid">The endpoint UID.</param>
        ///-------------------------------------------------------------------------------------------------

        public static void Remove(Guid endpointUid)
        {
            _instance._Remove(endpointUid);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Removes the specified client from all endpoints and cleans up orphaned endpoints</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="clientUid">The client UID.</param>
        ///-------------------------------------------------------------------------------------------------

        public static void RemoveClient(Guid clientUid)
        {
            _instance._RemoveClient(clientUid);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Queue message.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="endpointUid">The endpoint UID.</param>
        /// <param name="message">    The message.</param>
        ///-------------------------------------------------------------------------------------------------

        public static void QueueMessage(Guid endpointUid, string message)
        {
            _instance._QueueMessage(endpointUid, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Queue message.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="urlPart">The URL part.</param>
        /// <param name="message">The message.</param>
        ///-------------------------------------------------------------------------------------------------

        public static void QueueMessage(string urlPart, string message)
        {
            // **** check for not having this url part **** 

            if (!_instance._urlPartEndpointDict.ContainsKey(urlPart))
            {
                return;
            }

            // **** pass to instance ****

            _instance._QueueMessage(_instance._urlPartEndpointDict[urlPart].Uid, message);
        }

        #endregion Class Interface . . .

        #region Instance Interface . . .

        #endregion Instance Interface . . .

        #region Internal Functions . . .

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Queue message.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="endpointUid">The endpoint UID.</param>
        /// <param name="message">    The message.</param>
        ///-------------------------------------------------------------------------------------------------

        private void _QueueMessage(Guid endpointUid, string message)
        {
            // **** traverse the clients that have this endpoint ****

            foreach (Guid clientUid in _endpointClientsDict[endpointUid])
            {
                // **** notifiy this client ****

                if (!ClientManager.TryGetClient(clientUid, out ClientInformation client))
                {
                    // **** cannot find client ****

                    continue;
                }

                // **** queue a message for this client ****

                client.MessageQ.Enqueue(message);
            }
        }


        ///-------------------------------------------------------------------------------------------------
        /// <summary>Removes the client described by clientUid.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="clientUid">The client UID.</param>
        ///-------------------------------------------------------------------------------------------------

        private void _RemoveClient(Guid clientUid)
        {
            // **** check to see if this client has any endpoints ****

            if (!_clientEndpointsDict.ContainsKey(clientUid))
            {
                return;
            }

            // **** get list of endpoints with this client ****

            HashSet<Guid> endpoints = _clientEndpointsDict[clientUid];

            // **** remove this client from each endpoint ****

            foreach (Guid endpointUid in endpoints)
            {
                _endpointClientsDict[endpointUid].Remove(clientUid);

                // **** check for this endpoint being orphaned ****

                if (_endpointClientsDict[endpointUid].Count == 0)
                {
                    _Remove(endpointUid);
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Removes the given endpointUid.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">The endpoint UID.</param>
        ///-------------------------------------------------------------------------------------------------

        private void _Remove(Guid clientUid, Guid endpointUid)
        {
            // **** check to see if we have this client and endpoint ****

            if ((!_clientEndpointsDict.ContainsKey(clientUid)) || 
                (!_clientEndpointsDict.ContainsKey(endpointUid)))
            {
                return;
            }

            // **** remove this endpoint from this client ****

            _clientEndpointsDict[clientUid].Remove(endpointUid);

            // **** remove this client from this endpoint ****

            _endpointClientsDict[endpointUid].Remove(clientUid);

            // **** check for deleting this endpoint ****

            if (_endpointClientsDict[endpointUid].Count == 0)
            {
                // **** remove this endpoint completely ****

                _Remove(endpointUid);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Removes the given endpointUid.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="endpointUid">The endpoint UID.</param>
        ///-------------------------------------------------------------------------------------------------

        private void _Remove(Guid endpointUid)
        {
            // **** get list of clients with this endpoint ****

            HashSet<Guid> clients = _endpointClientsDict[endpointUid];

            // **** remove this endpoint from each client ****

            foreach (Guid clientUid in clients)
            {
                _clientEndpointsDict[clientUid].Remove(endpointUid);
            }

            // **** remove this endpoint ****

            _endpointClientsDict.Remove(endpointUid);
            
            if (!string.IsNullOrEmpty(_uidEndpointDict[endpointUid].UrlPart))
            {
                _urlPartEndpointDict.Remove(_uidEndpointDict[endpointUid].UrlPart);
            }

            _uidEndpointDict.Remove(endpointUid);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets endpoint list for client.</summary>
        ///
        /// <remarks>Gino Canessa, 7/29/2019.</remarks>
        ///
        /// <param name="clientUid">The client UID.</param>
        ///
        /// <returns>The endpoint list for client.</returns>
        ///-------------------------------------------------------------------------------------------------

        private List<EndpointInformation> _GetEndpointListForClient(Guid clientUid)
        {
            List<EndpointInformation> endpoints = new List<EndpointInformation>();

            // ****check for this client or no endpoints ****

            if ((!_clientEndpointsDict.ContainsKey(clientUid)) ||
                (_clientEndpointsDict[clientUid].Count == 0))
            {
                return endpoints;
            }

            // **** get the list of endpoints for this client ****

            foreach (Guid endpointUid in _clientEndpointsDict[clientUid])
            {
                // **** add this endpoint ****

                endpoints.Add(_uidEndpointDict[endpointUid]);
            }

            // **** return our list ****

            return endpoints;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Unregisters the client, either from a specific endpoint or from all endpoints</summary>
        ///
        /// <remarks>Gino Canessa, 7/26/2019.</remarks>
        ///
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">(Optional) The endpoint UID.</param>
        ///-------------------------------------------------------------------------------------------------

        private void _UnregisterClient(Guid clientUid, Guid? endpointUid = null)
        {
            // **** sanity checks ****

            if ((!_clientEndpointsDict.ContainsKey(clientUid)) || 
                (_clientEndpointsDict[clientUid].Count == 0))
            {
                // **** nothing to do ****

                return;
            }

            // **** determine if are removing the client from all endpoints ****

            if ((endpointUid == null) || (endpointUid == Guid.Empty))
            {
                // **** get list of endpoints for this client (cannot modify dict during traversal) ****

                Guid[] endpoints = _clientEndpointsDict[clientUid].ToArray<Guid>();

                // **** traverse endpoints ****

                foreach (Guid endpointGuid in endpoints)
                {
                    // **** remove this endpoint from this client ****

                    _clientEndpointsDict.Remove(endpointGuid);

                    // **** remove this client from this endpoint ****

                    _endpointClientsDict[endpointGuid].Remove(clientUid);
                }

                // **** done ****

                return;
            }

            // **** remove this endpoint from this client ****

            _clientEndpointsDict.Remove((Guid)endpointUid);

            // **** remove this client from this endpoint ****

            _endpointClientsDict[(Guid)endpointUid].Remove(clientUid);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Registers a client to an endpoint.</summary>
        ///
        /// <remarks>Gino Canessa, 7/26/2019.</remarks>
        ///
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">The endpoint UID.</param>
        ///-------------------------------------------------------------------------------------------------
        
        private void _RegisterClientForEndpoint(Guid clientUid, Guid endpointUid)
        {
            // **** check for first registration of this client ****

            if (!_clientEndpointsDict.ContainsKey(clientUid))
            {
                _clientEndpointsDict.Add(clientUid, new HashSet<Guid>());
            }

            // **** add this endpoint to this client (if necessary) ****

            if (!_clientEndpointsDict[clientUid].Contains(endpointUid))
            {
                _clientEndpointsDict[clientUid].Add(endpointUid);
            }

            // **** check for first registration of this endpoint ****

            if (!_endpointClientsDict.ContainsKey(endpointUid))
            {
                _endpointClientsDict.Add(endpointUid, new HashSet<Guid>());
            }

            // **** add this client to this endpoint (if necessary) ****

            if (!_endpointClientsDict[endpointUid].Contains(clientUid))
            {
                _endpointClientsDict[endpointUid].Add(clientUid);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Adds an or update.</summary>
        ///
        /// <remarks>Gino Canessa, 7/26/2019.</remarks>
        ///
        /// <param name="endpointUid"> The endpoint UID.</param>
        /// <param name="endpointType">Type of the endpoint.</param>
        /// <param name="url">         URL of the resource.</param>
        /// <param name="clientUid">   (Optional) The client UID.</param>
        ///-------------------------------------------------------------------------------------------------

        private void _AddOrUpdate(
                                    Guid endpointUid, 
                                    EndpointInformation.EndpointChannelType endpointType, 
                                    string urlPart, 
                                    Guid? clientUid = null
                                    )
        {
            EndpointInformation endpoint = EndpointInformation.Create(endpointUid, endpointType, urlPart);

            // **** add or update this ****

            _uidEndpointDict[endpointUid] = endpoint;

            if (!string.IsNullOrEmpty(urlPart))
            {
                _urlPartEndpointDict[urlPart] = endpoint;
            }

            // **** check to see if we have a client ****

            if ((clientUid != null) &&
                (clientUid != Guid.Empty))
            {
                // **** register this client to this endpoint ****

                _RegisterClientForEndpoint((Guid)clientUid, endpointUid);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Check or create instance.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///-------------------------------------------------------------------------------------------------

        private static void CheckOrCreateInstance()
        {
            if (_instance == null)
            {
                _instance = new EndpointManager();
            }
        }
        #endregion Internal Functions . . .

    }
}
