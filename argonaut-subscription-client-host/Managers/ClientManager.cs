// <copyright file="ClientManager.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// </copyright>

using argonaut_subscription_client_host.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace argonaut_subscription_client_host.Managers
{
    /// <summary>Manager singleton for clients.</summary>
    public class ClientManager
    {
        /// <summary>The instance for singleton pattern.</summary>
        private static ClientManager _instance;

        /// <summary>Dictionary of clients, by uid.</summary>
        private Dictionary<Guid, ClientInformation> _uidClientDict;

        /// <summary>
        /// Prevents a default instance of the <see cref="ClientManager"/> class from being created.
        /// </summary>
        private ClientManager()
        {
            // create our index objects

            _uidClientDict = new Dictionary<Guid, ClientInformation>();
        }
    
        /// <summary>Initializes this object.</summary>
        public static void Init()
        {
            CheckOrCreateInstance();
        }
    
        /// <summary>Gets client list.</summary>
        /// <returns>The client list.</returns>
        public static List<ClientInformation> GetClientList()
        {
            return _instance._uidClientDict.Values.ToList<ClientInformation>();
        }
    
        /// <summary>Adds or updates a client record.</summary>
        /// <param name="client">The client.</param>
        public static void AddOrUpdate(Guid clientUid, string fhirServerUrl)
        {
            _instance._uidClientDict[clientUid] = ClientInformation.Create(clientUid, fhirServerUrl);

            Console.WriteLine($"Added client >>>{clientUid}<<< total: {_instance._uidClientDict.Count}");
        }
    
        /// <summary>Removes the given clientGuid.</summary>
        /// <param name="clientGuid">The client Unique identifier to remove.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public static bool Remove(Guid clientUid)
        {
            if (!_instance._uidClientDict.ContainsKey(clientUid))
            {
                return false;
            }

            EndpointManager.RemoveClient(clientUid);

            _instance._uidClientDict.Remove(clientUid);

            Console.WriteLine($"Removed client >>>{clientUid}<<< total: {_instance._uidClientDict.Count}");

            return true;
        }

        /// <summary>Determine if 'clientUid' exists.</summary>
        /// <param name="clientUid">The client UID.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public static bool Exists(string clientUid)
        {
            if ((string.IsNullOrEmpty(clientUid)) ||
                (!Guid.TryParse(clientUid, out Guid clientGuid)))
            {
                return false;
            }

            return _instance._uidClientDict.ContainsKey(clientGuid);
        }

        /// <summary>Attempts to parse a GUID from the given string.</summary>
        /// <param name="clientUid"> The client UID.</param>
        /// <param name="clientGuid">[out] The client Unique identifier to remove.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public static bool TryParse(string clientUid, out Guid clientGuid)
        {
            clientGuid = Guid.Empty;

            if ((string.IsNullOrEmpty(clientUid)) ||
                (!Guid.TryParse(clientUid, out Guid parsedGuid)))
            {
                return false;
            }

            if (_instance._uidClientDict.ContainsKey(parsedGuid))
            {
                clientGuid = parsedGuid;
                return true;
            }

            return false;
        }
    
        /// <summary>Attempts to get client a ClientInformation from the given GUID.</summary>
        /// <param name="clientGuid">The client Unique identifier to remove.</param>
        /// <param name="client">    [out] The client.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public static bool TryGetClient(Guid clientGuid, out ClientInformation client)
        {
            if (_instance._uidClientDict.ContainsKey(clientGuid))
            {
                client = _instance._uidClientDict[clientGuid];
                return true;
            }

            client = null;
            return false;
        }
    
        /// <summary>Check or create instance.</summary>
        private static void CheckOrCreateInstance()
        {
            if (_instance == null)
            {
                _instance = new ClientManager();
            }
        }
    }
}

