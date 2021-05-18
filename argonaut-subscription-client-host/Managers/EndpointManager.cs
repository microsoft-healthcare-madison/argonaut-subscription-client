// <copyright file="EndpointManager.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using argonaut_subscription_client_host.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace argonaut_subscription_client_host.Managers
{
    /// <summary>Manager singleton for endpoints.</summary>
    public class EndpointManager
    {
        /// <summary>The instance for singleton pattern.</summary>
        private static EndpointManager _instance;

        /// <summary>Dictionary of endpoints, by uid.</summary>
        private Dictionary<Guid, EndpointInformation> _uidEndpointDict;

        /// <summary>Dictionary of endpoints, by URL part.</summary>
        private Dictionary<string, EndpointInformation> _urlPartEndpointDict;

        /// <summary>Dictionary of clients per endpoint guid</summary>
        private Dictionary<Guid, HashSet<Guid>> _endpointClientsDict;

        /// <summary>Dictionary of endpoints per client guid</summary>
        private Dictionary<Guid, HashSet<Guid>> _clientEndpointsDict;

        /// <summary>
        /// Prevents a default instance of the <see cref="EndpointManager"/>
        /// class from being created.
        /// </summary>
        private EndpointManager()
        {
            _uidEndpointDict = new Dictionary<Guid, EndpointInformation>();
            _urlPartEndpointDict = new Dictionary<string, EndpointInformation>();
            _endpointClientsDict = new Dictionary<Guid, HashSet<Guid>>();
            _clientEndpointsDict = new Dictionary<Guid, HashSet<Guid>>();
        }

        /// <summary>Initializes this object.</summary>
        public static void Init()
        {
            CheckOrCreateInstance();
        }

        /// <summary>Gets endpoint list.</summary>
        /// <returns>The endpoint list.</returns>
        public static List<EndpointInformation> GetEndpointList()
        {
            return _instance._uidEndpointDict.Values.ToList<EndpointInformation>();
        }

        /// <summary>Gets endpoint list for client.</summary>
        /// <param name="clientUid">The client UID.</param>
        /// <returns>The endpoint list for client.</returns>
        public static List<EndpointInformation> GetEndpointListForClient(Guid clientUid)
        {
            return _instance._GetEndpointListForClient(clientUid);
        }

        /// <summary>
        /// Attempts to get endpoint by UID an EndpointInformation from the given GUID.
        /// </summary>
        /// <param name="endpointUid">The endpoint UID.</param>
        /// <param name="endpoint">   [out] The endpoint.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public static bool TryGetEndpointByUid(Guid endpointUid, out EndpointInformation endpoint)
        {
            if (!_instance._uidEndpointDict.ContainsKey(endpointUid))
            {
                endpoint = null;
                return false;
            }

            endpoint = _instance._uidEndpointDict[endpointUid];
            return true;
        }

        /// <summary>
        /// Attempts to get endpoint by URL part an EndpointInformation from the given string.
        /// </summary>
        /// <param name="urlPart"> The URL part.</param>
        /// <param name="endpoint">[out] The endpoint.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public static bool TryGetEndpointByUrlPart(string urlPart, out EndpointInformation endpoint)
        {
            if (!_instance._urlPartEndpointDict.ContainsKey(urlPart))
            {
                endpoint = null;
                return false;
            }

            endpoint = _instance._urlPartEndpointDict[urlPart];
            return true;
        }

        /// <summary>Queries if an URL part is available.</summary>
        /// <param name="urlPart">The URL part.</param>
        /// <returns>True if the URL part is available, false if not.</returns>
        public static bool IsUrlPartAvailable(string urlPart)
        {
            if (string.IsNullOrEmpty(urlPart))
            {
                return false;
            }

            return !_instance._urlPartEndpointDict.ContainsKey(urlPart);
        }

        /// <summary>Determine if 'urlPart' exists.</summary>
        /// <param name="endpointUid">The endpoint UID.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public static bool Exists(Guid endpointUid)
        {
            return _instance._uidEndpointDict.ContainsKey(endpointUid);
        }

        /// <summary>Determine if 'urlPart' exists.</summary>
        /// <param name="urlPart">The URL part.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public static bool Exists(string urlPart)
        {
            return _instance._urlPartEndpointDict.ContainsKey(urlPart);
        }

        /// <summary>Adds or updates the requested endpoint.</summary>
        /// <param name="endpointUid"> The endpoint UID.</param>
        /// <param name="endpointType">Type of the endpoint.</param>
        /// <param name="url">         URL of the resource.</param>
        /// <param name="clientUid">   (Optional) The client UID.</param>
        public static void AddOrUpdate(Guid endpointUid, string endpointType, string url, Guid? clientUid = null)
        {
            // force to lower case for simplicity
            endpointType = endpointType.ToLower();

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

        /// <summary>Adds or updates the requested endpoint.</summary>
        /// <param name="endpointUid"> The endpoint UID.</param>
        /// <param name="endpointType">Type of the endpoint.</param>
        /// <param name="url">         URL of the resource.</param>
        /// <param name="clientUid">   (Optional) The client UID.</param>
        public static void AddOrUpdate(
            Guid endpointUid, 
            EndpointInformation.EndpointChannelType endpointType, 
            string url,
            Guid? clientUid = null)
        {
            _instance._AddOrUpdate(endpointUid, endpointType, url, clientUid);
        }

        /// <summary>Registers a client for an endpoint.</summary>
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">The endpoint UID.</param>
        public static void RegisterClientForEndpoint(Guid clientUid, Guid endpointUid)
        {
            _instance._RegisterClientForEndpoint(clientUid, endpointUid);
        }

        /// <summary>Unregister client, either from a specific endpoint or from all endpoints.</summary>
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">(Optional) The endpoint UID.</param>
        public static void UnregisterClient(Guid clientUid, Guid? endpointUid = null)
        {
            _instance._UnregisterClient(clientUid, endpointUid);
        }

        /// <summary>
        /// Removes the specified endpoint from the specified client and cleans up orphaned endpoints.
        /// </summary>
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">The endpoint UID.</param>
        public static void Remove(Guid clientUid, Guid endpointUid)
        {
            _instance._Remove(clientUid, endpointUid);
        }

        /// <summary>Removes the specified endpoint from all clients.</summary>
        /// <param name="endpointUid">The endpoint UID.</param>
        public static void Remove(Guid endpointUid)
        {
            _instance._Remove(endpointUid);
        }

        /// <summary>
        /// Removes the specified client from all endpoints and cleans up orphaned endpoints.
        /// </summary>
        /// <param name="clientGuid">The client UID.</param>
        public static void RemoveClient(Guid clientGuid)
        {
            _instance._RemoveClient(clientGuid);
        }

        /// <summary>Queue message.</summary>
        /// <param name="endpointGuid">The endpoint UID.</param>
        /// <param name="message">    The message.</param>
        public static void QueueMessage(Guid endpointGuid, string message)
        {
            _instance._QueueMessage(endpointGuid, message);
        }

        /// <summary>Queue message.</summary>
        /// <param name="urlPart">The URL part.</param>
        /// <param name="message">The message.</param>
        public static void QueueMessage(string urlPart, string message)
        {
            if (!_instance._urlPartEndpointDict.ContainsKey(urlPart))
            {
                return;
            }

            _instance._QueueMessage(_instance._urlPartEndpointDict[urlPart].Uid, message);
        }

        /// <summary>Enables the given endpoint UID.</summary>
        /// <param name="endpointUid">The endpoint UID.</param>
        /// <param name="endpoint">   [out] The endpoint.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public static bool TryEnable(Guid endpointUid, out EndpointInformation endpoint)
        {
            if (_instance._uidEndpointDict.ContainsKey(endpointUid))
            {
                _instance._uidEndpointDict[endpointUid].Enabled = true;
                endpoint = _instance._uidEndpointDict[endpointUid];
                return true;
            }

            endpoint = null;
            return false;
        }

        /// <summary>Disables the given endpoint UID.</summary>
        /// <param name="endpointGuid">The endpoint UID.</param>
        /// <param name="endpoint">   [out] The endpoint.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public static bool TryDisable(Guid endpointGuid, out EndpointInformation endpoint)
        {
            if (_instance._uidEndpointDict.ContainsKey(endpointGuid))
            {
                _instance._uidEndpointDict[endpointGuid].Enabled = false;
                endpoint = _instance._uidEndpointDict[endpointGuid];
                return true;
            }

            endpoint = null;
            return false;
        }

        /// <summary>Queue message.</summary>
        /// <param name="endpointGuid">The endpoint UID.</param>
        /// <param name="message">    The message.</param>
        private void _QueueMessage(Guid endpointGuid, string message)
        {
            // traverse the clients that have this endpoint
            foreach (Guid clientGuid in _endpointClientsDict[endpointGuid])
            {
                if (!WebsocketManager.QueueMessageForClient(clientGuid, message))
                {
                    RemoveClient(clientGuid);
                }
            }
        }

        /// <summary>Removes the client described by clientUid.</summary>
        /// <param name="clientGuid">The client UID.</param>
        private void _RemoveClient(Guid clientGuid)
        {
            // check to see if this client has any endpoints
            if (!_clientEndpointsDict.ContainsKey(clientGuid))
            {
                return;
            }

            // get list of endpoints with this client
            HashSet<Guid> endpoints = _clientEndpointsDict[clientGuid];

            // remove this client from each endpoint
            foreach (Guid endpointUid in endpoints)
            {
                _endpointClientsDict[endpointUid].Remove(clientGuid);

                // check for this endpoint being orphaned
                if (_endpointClientsDict[endpointUid].Count == 0)
                {
                    _Remove(endpointUid);
                }
            }
        }

        /// <summary>Removes the given endpointUid.</summary>
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">The endpoint UID.</param>
        private void _Remove(Guid clientUid, Guid endpointUid)
        {
            // check to see if we have this client and endpoint
            if ((!_clientEndpointsDict.ContainsKey(clientUid)) || 
                (!_clientEndpointsDict.ContainsKey(endpointUid)))
            {
                return;
            }

            // remove this endpoint from this client
            _clientEndpointsDict[clientUid].Remove(endpointUid);

            // remove this client from this endpoint
            _endpointClientsDict[endpointUid].Remove(clientUid);

            Console.WriteLine($"Removed endpoint >>>{endpointUid}<<< from client >>>{clientUid}<<<");

            // check for deleting this endpoint
            if (_endpointClientsDict[endpointUid].Count == 0)
            {
                // remove this endpoint completely
                _Remove(endpointUid);
            }
        }

        /// <summary>Removes the given endpointUid.</summary>
        /// <param name="endpointGuid">The endpoint UID.</param>
        private void _Remove(Guid endpointGuid)
        {
            // get list of clients with this endpoint
            HashSet<Guid> clients = _endpointClientsDict[endpointGuid];

            // remove this endpoint from each client
            foreach (Guid clientGuid in clients)
            {
                if (_clientEndpointsDict.ContainsKey(clientGuid))
                {
                    _clientEndpointsDict[clientGuid].Remove(endpointGuid);
                }
            }

            // remove this endpoint
            if (_endpointClientsDict.ContainsKey(endpointGuid))
            {
                _endpointClientsDict.Remove(endpointGuid);
            }

            if (_uidEndpointDict.ContainsKey(endpointGuid))
            {
                if (!string.IsNullOrEmpty(_uidEndpointDict[endpointGuid].UrlPart))
                {
                    _urlPartEndpointDict.Remove(_uidEndpointDict[endpointGuid].UrlPart);
                }

                _uidEndpointDict.Remove(endpointGuid);
            }

            Console.WriteLine($"Removed endpoint >>>{endpointGuid}<<< total: {_endpointClientsDict.Count}");
        }

        /// <summary>Gets endpoint list for client.</summary>
        /// <param name="clientUid">The client UID.</param>
        /// <returns>The endpoint list for client.</returns>
        private List<EndpointInformation> _GetEndpointListForClient(Guid clientUid)
        {
            List<EndpointInformation> endpoints = new List<EndpointInformation>();

            // check for this client or no endpoints
            if ((!_clientEndpointsDict.ContainsKey(clientUid)) ||
                (_clientEndpointsDict[clientUid].Count == 0))
            {
                return endpoints;
            }

            // get the list of endpoints for this client
            foreach (Guid endpointUid in _clientEndpointsDict[clientUid])
            {
                // add this endpoint

                endpoints.Add(_uidEndpointDict[endpointUid]);
            }

            return endpoints;
        }

        /// <summary>
        /// Unregisters the client, either from a specific endpoint or from all endpoints.
        /// </summary>
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">(Optional) The endpoint UID.</param>
        private void _UnregisterClient(Guid clientUid, Guid? endpointUid = null)
        {
            if ((!_clientEndpointsDict.ContainsKey(clientUid)) || 
                (_clientEndpointsDict[clientUid].Count == 0))
            {
                return;
            }

            // determine if are removing the client from all endpoints
            if ((endpointUid == null) || (endpointUid == Guid.Empty))
            {
                // get list of endpoints for this client (cannot modify dict during traversal)
                Guid[] endpoints = _clientEndpointsDict[clientUid].ToArray<Guid>();

                foreach (Guid endpointGuid in endpoints)
                {
                    _clientEndpointsDict.Remove(endpointGuid);
                    _endpointClientsDict[endpointGuid].Remove(clientUid);
                }

                return;
            }

            _clientEndpointsDict.Remove((Guid)endpointUid);
            _endpointClientsDict[(Guid)endpointUid].Remove(clientUid);
        }

        /// <summary>Registers a client to an endpoint.</summary>
        /// <param name="clientUid">  The client UID.</param>
        /// <param name="endpointUid">The endpoint UID.</param>        
        private void _RegisterClientForEndpoint(Guid clientUid, Guid endpointUid)
        {
            // check for first registration of this client
            if (!_clientEndpointsDict.ContainsKey(clientUid))
            {
                _clientEndpointsDict.Add(clientUid, new HashSet<Guid>());
            }

            // add this endpoint to this client (if necessary)
            if (!_clientEndpointsDict[clientUid].Contains(endpointUid))
            {
                _clientEndpointsDict[clientUid].Add(endpointUid);
            }

            // check for first registration of this endpoint
            if (!_endpointClientsDict.ContainsKey(endpointUid))
            {
                _endpointClientsDict.Add(endpointUid, new HashSet<Guid>());
            }

            // add this client to this endpoint (if necessary)
            if (!_endpointClientsDict[endpointUid].Contains(clientUid))
            {
                _endpointClientsDict[endpointUid].Add(clientUid);
            }
        }

        /// <summary>Adds or updates an endpoint.</summary>
        /// <remarks>Gino Canessa, 7/26/2019.</remarks>
        /// <param name="endpointUid"> The endpoint UID.</param>
        /// <param name="endpointType">Type of the endpoint.</param>
        /// <param name="urlPart">     The URL part.</param>
        /// <param name="clientUid">   (Optional) The client UID.</param>
        private void _AddOrUpdate(
            Guid endpointUid, 
            EndpointInformation.EndpointChannelType endpointType, 
            string urlPart, 
            Guid? clientUid = null)
        {
            EndpointInformation endpoint = new EndpointInformation(endpointUid, endpointType, urlPart);

            _uidEndpointDict[endpointUid] = endpoint;

            if (!string.IsNullOrEmpty(urlPart))
            {
                _urlPartEndpointDict[urlPart] = endpoint;
            }

            Console.WriteLine($"Created endpoint >>>{endpointUid}<<< total: {_endpointClientsDict.Count}");

            if ((clientUid != null) &&
                (clientUid != Guid.Empty))
            {
                _RegisterClientForEndpoint((Guid)clientUid, endpointUid);
                Console.WriteLine($"Added endpoint >>>{endpointUid}<<< to client: >>>{clientUid}<<<");
            }
        }

        /// <summary>Check or create instance.</summary>
        private static void CheckOrCreateInstance()
        {
            if (_instance == null)
            {
                _instance = new EndpointManager();
            }
        }
    }
}
