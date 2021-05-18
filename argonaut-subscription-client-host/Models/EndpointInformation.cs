// <copyright file="EndpointInformation.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace argonaut_subscription_client_host.Models
{
    /// <summary>Information about the endpoint.</summary>
    public class EndpointInformation
    {
        /// <summary>Values that represent endpoint channel types.</summary>
        public enum EndpointChannelType: int {
            RestHook = 0,
            WebSocket,
            ServerSideEvent
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointInformation"/> class.
        /// </summary>
        public EndpointInformation()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EndpointInformation"/> class.</summary>
        /// <param name="uid">        The UID.</param>
        /// <param name="channelType">The type of the channel.</param>
        /// <param name="urlPart">    The URL.</param>
        public EndpointInformation(
            Guid uid,
            EndpointChannelType channelType,
            string urlPart)
        {
            Uid = uid;
            ChannelType = channelType;
            UrlPart = urlPart;
            Enabled = true;
        }

        /// <summary>Gets or sets the unique id for this endpoint</summary>
        public Guid Uid { get; set; }
    
        /// <summary>Gets or sets the type of the channel.</summary>
        public EndpointChannelType ChannelType { get; set; }
    
        /// <summary>Gets or sets URL part for this endpoint.</summary>
        public string UrlPart { get; set; }

        /// <summary>Gets or sets a value indicating whether this endpoint is enabled.</summary>
        public bool Enabled { get; set; }
    }
}
