using argonaut_subscription_client_host.Managers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.RegularExpressions;

namespace argonaut_subscription_client_host
{
    class Program
    {
        /// <summary>A Regex pattern to filter proper base URLs for WebHost.</summary>
        private const string _regexBaseUrlMatch = @"(http[s]*:\/\/[A-Za-z0-9\.]*(:\d+)*)";

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Gets or sets the configuration.</summary>
        ///
        /// <value>The configuration.</value>
        ///-------------------------------------------------------------------------------------------------

        public static IConfiguration Configuration { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Main entry-point for this application.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <param name="args">An array of command-line argument strings.</param>
        ///-------------------------------------------------------------------------------------------------

        public static void Main(string[] args)
        {
            // **** setup our configuration (command line > environment > appsettings.json) ****

            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build()
                ;

            // **** update configuration to make sure listen url is properly formatted ****

            Regex regex = new Regex(_regexBaseUrlMatch);
            Match match = regex.Match(Configuration["Client_Listen_Url"]);
            Configuration["Client_Listen_Url"] = match.ToString();

            // **** initialize managers ****

            ClientManager.Init();
            EndpointManager.Init();

            // **** create our web host ****

            CreateWebHostBuilder(args).Build().Run();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>Creates web host builder.</summary>
        ///
        /// <remarks>Gino Canessa, 7/18/2019.</remarks>
        ///
        /// <param name="args">An array of command-line argument strings.</param>
        ///
        /// <returns>The new web host builder.</returns>
        ///-------------------------------------------------------------------------------------------------

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls(Configuration["Client_Listen_Url"])
                .UseKestrel()
                .UseStartup<UiHostStartup>()
                ;
    }
}
