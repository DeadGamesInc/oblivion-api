/*
 *  OblivionAPI :: Program
 *
 *  This class is responsible for creating and starting the application host container.
 * 
 */

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using OblivionAPI.Config;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OblivionAPI {
    public static class Program {
        public static async Task Main() {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Error)
                .WriteTo.Console()
                .CreateLogger();

            Globals.LISTEN_PORT = Environment.GetEnvironmentVariable("PORT") ?? "5001";

            if (Environment.GetEnvironmentVariable("CACHE_TIME") != null) 
                Globals.CACHE_TIME = Convert.ToUInt32(Environment.GetEnvironmentVariable("CACHE_TIME"));
            
            if (Environment.GetEnvironmentVariable("REFRESH_TIME") != null) 
                Globals.REFRESH_TIME = Convert.ToUInt32(Environment.GetEnvironmentVariable("REFRESH_TIME"));
            
            if (Environment.GetEnvironmentVariable("THROTTLE_WAIT") != null) 
                Globals.THROTTLE_WAIT = Convert.ToInt32(Environment.GetEnvironmentVariable("THROTTLE_WAIT"));

            if (Environment.GetEnvironmentVariable("REDUCED_IMAGE_WIDTH") != null)
                Globals.REDUCED_IMAGE_WIDTH = Convert.ToInt32(Environment.GetEnvironmentVariable("REDUCED_IMAGE_WIDTH"));

            if (Environment.GetEnvironmentVariable("REDUCED_IMAGE_HEIGHT") != null)
                Globals.REDUCED_IMAGE_HEIGHT = Convert.ToInt32(Environment.GetEnvironmentVariable("REDUCED_IMAGE_HEIGHT"));

            if (Environment.GetEnvironmentVariable("IMAGE_CACHE_PREFIX") != null)
                Globals.IMAGE_CACHE_PREFIX = Environment.GetEnvironmentVariable("IMAGE_CACHE_PREFIX");
            
            if (!Directory.Exists(Globals.WEB_ROOT)) Directory.CreateDirectory(Globals.WEB_ROOT);
            if (!Directory.Exists(Globals.IMAGE_CACHE_DIR)) Directory.CreateDirectory(Globals.IMAGE_CACHE_DIR);
            
            var host = CreateHost();

            try {
                Log.Logger.Information("Dead Games API is starting");
                await host.RunAsync();
            } catch (Exception error) {
                Log.Logger.Error(error, "An exception occured in host execution");
            }
        }

        private static IHost CreateHost() =>
            Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder
                        .UseContentRoot(Globals.WEB_ROOT)
                        .UseStartup<Startup>()
                        .UseUrls($"http://*:{Globals.LISTEN_PORT}")
                        .UseKestrel();
                })
                .UseSerilog()
                .Build();
    }
}
