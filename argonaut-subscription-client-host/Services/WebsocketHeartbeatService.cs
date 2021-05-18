// <copyright file="WebsocketHeartbeatService.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using argonaut_subscription_client_host.Managers;
using argonaut_subscription_client_host.Models;
using Hl7.Fhir.Utility;
using Microsoft.Extensions.Hosting;

namespace argonaut_subscription_client_host.Services
{
    /// <summary>A service for accessing web socket keep alives information.</summary>
    public class WebsocketHeartbeatService : IHostedService, IDisposable
    {
        /// <summary>The callback timer.</summary>
        private Timer _timer;
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebsocketHeartbeatService"/> class.
        /// </summary>
        public WebsocketHeartbeatService()
        {
        }

        private void CheckAndSendHeartbeats(object state)
        {
            long currentTicks = DateTime.Now.Ticks;
            string heartbeatTime = string.Format(CultureInfo.InvariantCulture, "{0:o}", DateTime.Now.ToUniversalTime());

            WebsocketManager.ProcessKeepalives(currentTicks, heartbeatTime);
        }

        /// <summary>Triggered when the application host is ready to start the service.</summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns>An asynchronous result.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Websocket Keepalive Service Started.");

            _timer = new Timer(
                CheckAndSendHeartbeats,
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        /// <summary>Triggered when the application host is performing a graceful shutdown.</summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be
        ///  graceful.</param>
        /// <returns>An asynchronous result.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the
        /// argonaut_subscription_server_proxy.Services.WebsocketHeartbeatService and optionally releases
        /// the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to
        ///  release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _timer?.Dispose();
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
