// <copyright file="Program.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// </copyright>

using System;
using System.Text.RegularExpressions;
using argonaut_subscription_client_host.Managers;
using argonaut_subscription_client_host.Models;
using argonaut_subscription_client_host.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace argonaut_subscription_client_host
{
    /// <summary>A program.</summary>
    public static class Program
    {
        /// <summary>A Regex pattern to filter proper base URLs for WebHost.</summary>
        private const string _regexBaseUrlMatch = @"(http[s]*:\/\/[A-Za-z0-9\.]*(:\d+)*)";
    
        /// <summary>Gets or sets the configuration.</summary>
        public static IConfiguration Configuration { get; set; }

        /// <summary>Main entry-point for this application.</summary>
        /// <param name="args">An array of command-line argument strings.</param>
        public static void Main(string[] args)
        {
            // setup our configuration (command line > environment > appsettings.json)
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // update configuration to make sure listen url is properly formatted
            Regex regex = new Regex(_regexBaseUrlMatch);
            Match match = regex.Match(Configuration["Client_Internal_Url"]);
            Configuration["Client_Internal_Url"] = match.ToString();

            // initialize managers
            ClientManager.Init();
            EndpointManager.Init();
            WebsocketManager.Init();

            // create our service host
            CreateHostBuilder(args).Build().StartAsync();

            // create our web host
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>Creates host builder.</summary>
        /// <returns>The new host builder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<WebsocketHeartbeatService>();
                });

        /// <summary>Creates web host builder.</summary>
        /// <param name="args">An array of command-line argument strings.</param>
        /// <returns>The new web host builder.</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls(Configuration["Client_Internal_Url"])
                .UseKestrel()
                .UseStartup<Startup>();
    }
}
