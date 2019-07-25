using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.FileProviders;
using System.IO;
using argonaut_subscription_client_host.Handlers;
using System.Reflection;

namespace argonaut_subscription_client_host
{
    public class UiHostStartup
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>Configure services for the web application.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <param name="services">The services.</param>
        ///-------------------------------------------------------------------------------------------------

        public void ConfigureServices(IServiceCollection services)
        {
            // **** add MVC to our services ****

            services.AddMvc();

            // **** because we are loading the web host manually, we need to force it to load our local assemblies ****

            services.AddMvc().AddApplicationPart(Assembly.GetExecutingAssembly());

            // **** inject the configuration singleton into our services ****

            services.AddSingleton<IConfiguration>(Program.Configuration);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Configures a web application</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <param name="app">The application.</param>
        /// <param name="env">The environment.</param>
        ///-------------------------------------------------------------------------------------------------

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // **** we want to essentially disable CORS ****

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                //.AllowCredentials()
                );

            // **** grab the configured source path for the UI ****

            string uiSourcePath = Program.Configuration["UI_Source_Path"];

            if (!Path.IsPathRooted(uiSourcePath))
            {
                uiSourcePath = Path.Combine(Directory.GetCurrentDirectory(), uiSourcePath);
            }

            // **** flag we are allowing static files for the UI (if necessary) ****

            if (Directory.Exists(uiSourcePath))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(uiSourcePath),
                    RequestPath = "/ui"
                });
            }

            // **** enable websockets ****

            app.UseWebSockets();

            // **** flag we want MVC (for API routing, Controllers are decorated with routes) ****

            app.UseMvc();
          
            // **** enable custom middleware to handle websocket requests ****

            app.UseMiddleware<ClientWebSocketHandler>("/websockets");
        }
    }
}
