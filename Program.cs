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

namespace OblivionAPI {
    public static class Program {
        public static void Main() {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .WriteTo.Console()
                .CreateLogger();

            Globals.LISTEN_PORT = Environment.GetEnvironmentVariable("PORT") ?? "5001";

            if (Environment.GetEnvironmentVariable("CACHE_TIME") != null) 
                Globals.CACHE_TIME = Convert.ToUInt32(Environment.GetEnvironmentVariable("CACHE_TIME"));
            
            if (Environment.GetEnvironmentVariable("REFRESH_TIME") != null) 
                Globals.REFRESH_TIME = Convert.ToUInt32(Environment.GetEnvironmentVariable("REFRESH_TIME"));
            
            if (Environment.GetEnvironmentVariable("THROTTLE_WAIT") != null) 
                Globals.THROTTLE_WAIT = Convert.ToInt32(Environment.GetEnvironmentVariable("THROTTLE_WAIT"));

            var host = CreateHost();

            try {
                Log.Logger.Information("Dead Games API is starting");
                host.Run();
            } catch (Exception error) {
                Log.Logger.Error(error, "An exception occured in host execution");
            }
        }

        private static IHost CreateHost() =>
            Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseUrls($"http://*:{Globals.LISTEN_PORT}")
                        .UseKestrel();
                })
                .UseSerilog()
                .Build();
    }
}
