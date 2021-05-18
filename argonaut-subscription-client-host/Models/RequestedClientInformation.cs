// <copyright file="RequestedClientInformation.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace argonaut_subscription_client_host.Models
{
    /// <summary>Minimal structure for requesting a new client.</summary>
    public class RequestedClientInformation
    {
        /// <summary>Gets or sets a unique identifier of the client.</summary>
        [JsonProperty("uid")]
        public string Uid { get; set; }

        /// <summary>Gets or sets URL of the FHIR server.</summary>
        [JsonProperty("fhirServerUrl")]
        public string FhirServerUrl { get; set; }
    }
}
