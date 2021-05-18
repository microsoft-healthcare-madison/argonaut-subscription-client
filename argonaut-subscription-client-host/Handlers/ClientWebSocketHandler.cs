// <copyright file="ClientWebSocketHandler.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using argonaut_subscription_client_host.Managers;
using argonaut_subscription_client_host.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace argonaut_subscription_client_host.Handlers
{
    /// <summary>A client web socket handler.</summary>
    public class ClientWebSocketHandler
    {
        /// <summary>Size of the message buffer.</summary>
        private const int _messageBufferSize = 1024 * 8;            // 8 KB

        /// <summary>The send sleep delay in milliseconds.</summary>
        private const int _sendSleepDelayMs = 100;

        /// <summary>The keepalive timeout in ticks.</summary>
        private const long _keepaliveTimeoutTicks = 10 * TimeSpan.TicksPerSecond;         // 10 seconds

        /// <summary>   The configuration. </summary>
        private readonly IConfiguration _config;

        /// <summary>The next delegate.</summary>
        private readonly RequestDelegate _nextDelegate;

        /// <summary>A token that allows processing to be cancelled.</summary>
        private readonly CancellationToken _applicationStopping;

        /// <summary>Dictionary of client message timeouts.</summary>
        private ConcurrentDictionary<Guid, long> _clientMessageTimeoutDict;

        /// <summary>URL to match.</summary>
        private readonly string _matchUrl;

        /// <summary>Number of websockets.</summary>
        private int _websocketCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientWebSocketHandler"/> class.
        /// </summary>
        /// <param name="nextDelegate">  The next delegate in the process chain.</param>
        /// <param name="appLifetime">   The application lifetime.</param>
        /// <param name="iConfiguration">Reference to application configuration.</param>
        /// <param name="matchUrl">      URL to match.</param>
        public ClientWebSocketHandler(
            RequestDelegate nextDelegate,
            IHostApplicationLifetime appLifetime,
            IConfiguration iConfiguration,
            string matchUrl)
        {
            _config = iConfiguration;
            _nextDelegate = nextDelegate;
            _applicationStopping = appLifetime.ApplicationStopping;
            _matchUrl = matchUrl;

            _clientMessageTimeoutDict = new ConcurrentDictionary<Guid, long>();

            _websocketCount = 0;
        }

        /// <summary>Executes the asynchronous on a different thread, and waits for the result.</summary>
        /// <param name="context">The context.</param>
        /// <returns>An asynchronous result.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // check for requests to our URL
            if (!context.Request.Path.Equals(_matchUrl, StringComparison.OrdinalIgnoreCase))
            {
                // pass to next caller in chain
                await _nextDelegate.Invoke(context);
                return;
            }

            // check for not being a WebSocket request
            if (!context.WebSockets.IsWebSocketRequest)
            {
                Console.WriteLine($" <<< Received non-websocket request at: {_matchUrl}");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            // check the client uid
            if (!context.Request.Query.ContainsKey("uid") ||
                !ClientManager.TryParse(context.Request.Query["uid"], out Guid clientGuid))
            {
                Console.WriteLine($" <<< Websocket request does not contain a valid client uid");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            // accept this connection
            await AcceptAndProcessWebSocket(context, clientGuid);
        }

        /// <summary>Tests queueing messages.</summary>
        /// <param name="clientGuid">Unique identifier for the client.</param>
        private void TestQueueingMessages(Guid clientGuid)
        {
            bool done = false;
            long messageNumber = 0;

            while (!done)
            {
                // check for no client
                if (!ClientManager.TryGetClient(clientGuid, out ClientInformation client))
                {
                    done = true;
                    continue;
                }

                // queue a message for this client
                client.MessageQ.Enqueue($"Test message: {messageNumber++}, {DateTime.Now}");

                // wait a couple of seconds
                Thread.Sleep(2000);
            }
        }

        /// <summary>Accept and process a web socket connection.</summary>
        /// <param name="context">   The context.</param>
        /// <param name="clientGuid">Unique identifier for the client.</param>
        /// <returns>An asynchronous result.</returns>
        private async Task AcceptAndProcessWebSocket(
            HttpContext context,
            Guid clientGuid)
        {
            // prevent WebSocket errors from bubbling up
            try
            {
                // accept the connection
                using (var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false))
                {
                    // track we have a new websocket
                    Interlocked.Increment(ref _websocketCount);

                    Console.WriteLine($"Added websocket for client >>>{clientGuid}<<< current count: {_websocketCount}");

                    // add our client to the dictionary to send keepalives, force one to start
                    _clientMessageTimeoutDict.TryAdd(clientGuid, 0);

                    // create a cancellation token source so we can cancel our read/write tasks
                    using (CancellationTokenSource processCancelSource = new CancellationTokenSource())

                    // link our local close with the application lifetime close for simplicity
                    using (CancellationTokenSource linkedSource =
                        CancellationTokenSource.CreateLinkedTokenSource(_applicationStopping, processCancelSource.Token))
                    {
                        CancellationToken cancelToken = linkedSource.Token;

                        Task[] webSocketTasks = new Task[2];

                        // create send task
                        webSocketTasks[0] = Task.Run(async () =>
                        {
                            try
                            {
                                await WriteClientMessages(webSocket, clientGuid, cancelToken).ConfigureAwait(false);
                            }
                            finally
                            {
                                // cancel read if write task has exited
                                processCancelSource.Cancel();
                            }
                        });

                        // create receive task
                        webSocketTasks[1] = Task.Run(async () =>
                        {
                            try
                            {
                                await ReadClientMessages(webSocket, clientGuid, cancelToken).ConfigureAwait(false);
                            }
                            finally
                            {
                                // cancel write if read task has exited
                                processCancelSource.Cancel();
                            }
                        });

                        // start tasks and wait for them to complete
                        await Task.WhenAll(webSocketTasks).ConfigureAwait(false);

                        // close our web socket (do not allow cancel)
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.EndpointUnavailable,
                            "Connection closing",
                            CancellationToken.None).ConfigureAwait(false);
                    }
                }
            }
            catch (WebSocketException wsEx)
            {
                Console.WriteLine($" <<< caught exception: {wsEx.Message}");
            }
            finally
            {
                // track we have closed a websocket
                Interlocked.Decrement(ref _websocketCount);

                // remove this client - disconnected
                ClientManager.Remove(clientGuid);
                Console.WriteLine($"Removed websocket for client >>>{clientGuid}<<< current count: {_websocketCount}");
            }

            return;
        }

        /// <summary>Write client messages.</summary>
        /// <param name="webSocket">  The web socket.</param>
        /// <param name="clientGuid"> Unique identifier for the client.</param>
        /// <param name="cancelToken">A token that allows processing to be cancelled.</param>
        /// <returns>An asynchronous result.</returns>
        private async Task WriteClientMessages(
            WebSocket webSocket, 
            Guid clientGuid, 
            CancellationToken cancelToken)
        {
            // get the client object
            if (!ClientManager.TryGetClient(clientGuid, out ClientInformation client))
            {
                return;
            }

            // loop until cancelled
            while (!cancelToken.IsCancellationRequested)
            {
                // do not bubble errors here
                try
                {
                    // check for a message
                    if (!client.MessageQ.TryDequeue(out string message))
                    {
                        // wait and prevent exceptions
                        await Task.Delay(_sendSleepDelayMs, cancelToken)
                            .ContinueWith(_ => Task.CompletedTask);

                        continue;
                    }

                    // grab a byte buffer of our data
                    byte[] buffer = Encoding.UTF8.GetBytes(message);

                    // send this message
                    await webSocket.SendAsync(
                        buffer,
                        WebSocketMessageType.Text,
                        true,
                        cancelToken);

                    // update our keepalive timeout
                    _clientMessageTimeoutDict[clientGuid] = DateTime.Now.Ticks + _keepaliveTimeoutTicks;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"ClientWebSocketHandler.WriteClientMessages" +
                        $" <<< client: {clientGuid} caught exception: {ex.Message}");

                    // this socket is borked, exit
                    break;
                }
            }
        }

        /// <summary>Reads client messages.</summary>
        /// <param name="webSocket">  The web socket.</param>
        /// <param name="clientGuid"> Unique identifier for the client.</param>
        /// <param name="cancelToken">A token that allows processing to be cancelled.</param>
        /// <returns>An asynchronous result.</returns>
        private async Task ReadClientMessages(
            WebSocket webSocket, 
            Guid clientGuid, 
            CancellationToken cancelToken)
        {
            // create our receive buffer
            byte[] buffer = new byte[_messageBufferSize];
            int offset;
            int count;

            WebSocketReceiveResult result;

            // loop until cancelled
            while (!cancelToken.IsCancellationRequested)
            {
                // reset buffer offset
                offset = 0;

                // do not bubble errors here
                try
                {
                    // read a message
                    do
                    {
                        count = _messageBufferSize - offset;
                        result = await webSocket.ReceiveAsync(
                                    new ArraySegment<byte>(
                                        buffer,
                                        offset,
                                        count),
                                    cancelToken);
                    }
                    while (!result.EndOfMessage);

                    // process this message
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    Console.WriteLine(
                        $"ClientWebSocketHandler.ReadClientMessages" +
                        $" <<< client: {clientGuid} received: {Encoding.UTF8.GetString(buffer)}");
                }

                // keep looping
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"ClientWebSocketHandler.ReadClientMessages" +
                        $" <<< client: {clientGuid} caught exception: {ex.Message}");

                    // this socket is borked, exit
                    break;
                }
            }
        }
    }
}
