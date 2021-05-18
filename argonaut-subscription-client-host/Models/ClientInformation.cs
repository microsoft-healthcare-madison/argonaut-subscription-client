// <copyright file="ClientInformation.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace argonaut_subscription_client_host.Models
{
    /// <summary>Information about the client.</summary>
    public class ClientInformation
    {
        /// <summary>Gets or sets a unique identifier of the client.</summary>
        [JsonProperty("uid")]
        public Guid Uid { get; set; }

        /// <summary>Gets or sets URL of the FHIR server.</summary>
        [JsonProperty("fhirServerUrl")]
        public string FhirServerUrl { get; set; }

        /// <summary>Gets or sets the endpoints.</summary>
        [JsonIgnoreAttribute]
        public Dictionary<Guid, Guid> ClientEndpoints { get; set; }

        /// <summary>Gets or sets the message queue for this client.</summary>
        [JsonIgnoreAttribute]
        public ConcurrentQueue<string> MessageQ { get; set; }

        public ClientInformation()
        {
            ClientEndpoints = new Dictionary<Guid, Guid>();
            MessageQ = new ConcurrentQueue<string>();
        }

        /// <summary>Creates a new ClientInformation.</summary>
        /// <param name="uid">          Unique identifier of the client.</param>
        /// <param name="fhirServerUrl">The fhir server URL.</param>
        /// <returns>A ClientInformation.</returns>
        public static ClientInformation Create(Guid uid, string fhirServerUrl)
        {
            return new ClientInformation()
            {
                Uid = uid,
                FhirServerUrl = fhirServerUrl,
                ClientEndpoints = new Dictionary<Guid, Guid>(),
                MessageQ = new ConcurrentQueue<string>(),
            };
        }
    }
}
