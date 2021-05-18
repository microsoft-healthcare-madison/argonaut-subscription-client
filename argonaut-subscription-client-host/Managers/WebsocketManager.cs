// <copyright file="WebsocketManager.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// </copyright>

using argonaut_subscription_client_host.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace argonaut_subscription_client_host.Managers
{
    /// <summary>Manager for websockets.</summary>
    public class WebsocketManager
    {
        /// <summary>The keepalive timeout in ticks.</summary>
        private const long _keepaliveTimeoutTicks = 29 * TimeSpan.TicksPerSecond;         // 29 seconds

        /// <summary>The instance for singleton pattern.</summary>
        private static WebsocketManager _instance;

        /// <summary>The FHIR R4 clients and timeouts.</summary>
        private ConcurrentDictionary<Guid, long> _clientsAndTimeouts;

        /// <summary>
        /// Prevents a default instance of the
        /// <see cref="WebsocketManager"/> class from being created.
        /// </summary>
        private WebsocketManager()
        {
            _clientsAndTimeouts = new ConcurrentDictionary<Guid, long>();
        }

        /// <summary>Initializes this object.</summary>
        public static void Init()
        {
            // make an instance
            CheckOrCreateInstance();
        }

        /// <summary>Registers the client described by client.</summary>
        /// <param name="client">The client.</param>
        public static void RegisterClient(ClientInformation client)
        {
            if (client == null)
            {
                return;
            }

            if (!_instance._clientsAndTimeouts.ContainsKey(client.Uid))
            {
                _instance._clientsAndTimeouts.TryAdd((Guid)client.Uid, DateTime.Now.Ticks + _keepaliveTimeoutTicks);
            }
        }

        /// <summary>Unregisters the client described by GUID.</summary>
        /// <param name="clientGuid">Unique identifier for the client.</param>
        public static void UnregisterClient(Guid clientGuid)
        {
            if (clientGuid == Guid.Empty)
            {
                return;
            }

            if (_instance._clientsAndTimeouts.ContainsKey(clientGuid))
            {
                _instance._clientsAndTimeouts.TryRemove(clientGuid, out long _);
            }
        }

        /// <summary>Process the keepalives.</summary>
        /// <param name="currentTicks">The current ticks.</param>
        /// <param name="timeString">  The time string.</param>
        public static void ProcessKeepalives(long currentTicks, string timeString)
        {
            List<Guid> clientsToRemove = new List<Guid>();

            // traverse the dictionary looking for clients we need to send messages to
            foreach (KeyValuePair<Guid, long> kvp in _instance._clientsAndTimeouts)
            {
                // check timeout
                if (currentTicks > kvp.Value)
                {
                    // enqueue a message for this client
                    if (ClientManager.TryGetClient(kvp.Key, out ClientInformation client))
                    {
                        // enqueue a keepalive message
                        client.MessageQ.Enqueue($"keepalive {timeString}");
                    }
                    else
                    {
                        // client is gone, stop sending (cannot remove inside iterator)
                        clientsToRemove.Add(kvp.Key);
                    }
                }
            }

            if (clientsToRemove.Count > 0)
            {
                foreach (Guid clientGuid in clientsToRemove)
                {
                    _instance._clientsAndTimeouts.TryRemove(clientGuid, out _);
                }
            }
        }

        /// <summary>Updates the timeout for sent message described by clientGuid.</summary>
        /// <param name="clientGuid">Unique identifier for the client.</param>
        public static void UpdateTimeoutForSentMessage(Guid clientGuid)
        {
            // update our keepalive timeout
            _instance._clientsAndTimeouts[clientGuid] = DateTime.Now.Ticks + _keepaliveTimeoutTicks;
        }

        /// <summary>Queue messages for client.</summary>
        /// <param name="clientGuid">Unique identifier for the client.</param>
        /// <param name="message">   The message.</param>
        public static bool QueueMessageForClient(
            Guid clientGuid,
            string message)
        {
            if (!ClientManager.TryGetClient(clientGuid, out ClientInformation client))
            {
                if (_instance._clientsAndTimeouts.ContainsKey(clientGuid))
                {
                    _instance._clientsAndTimeouts.TryRemove(clientGuid, out long _);
                }

                return false;
            }

            // add this message to this client's queue (caller should have set it up correctly)
            client.MessageQ.Enqueue(message);

            return true;
        }

        /// <summary>Check or create instance.</summary>
        private static void CheckOrCreateInstance()
        {
            if (_instance == null)
            {
                _instance = new WebsocketManager();
            }
        }
    }
}
