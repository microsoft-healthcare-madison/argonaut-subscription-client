// <copyright file="Startup.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using argonaut_subscription_client_host.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace argonaut_subscription_client_host
{
    /// <summary>A web host startup.</summary>
    public class Startup
    {
        /// <summary>Configure services for the web application.</summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => options.EnableEndpointRouting = false);

            // because we are loading the web host manually, we need to force it to load our local assemblies
            services.AddMvc().AddApplicationPart(Assembly.GetExecutingAssembly());

            // inject the configuration singleton into our services
            services.AddSingleton<IConfiguration>(Program.Configuration);
        }

        /// <summary>Configures a web application.</summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // we want to essentially disable CORS
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                //.AllowCredentials()
                );

            // enable websockets
            app.UseWebSockets();

            // flag we want MVC (for API routing, Controllers are decorated with routes)
            app.UseMvc();

            // enable custom middleware to handle websocket requests
            app.UseMiddleware<ClientWebSocketHandler>("/websockets");
        }
    }
}
