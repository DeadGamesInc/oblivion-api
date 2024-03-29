/*
 *  OblivionAPI :: Program
 *
 *  This class is responsible for creating and starting the application host container.
 * 
 */

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace OblivionAPI; 

public static class Program {
    public static async Task Main() {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3} {Message}{NewLine}{Exception}")
            .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "api.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3} {Message}{NewLine}{Exception}")
            .CreateLogger();

        Globals.LISTEN_PORT = Environment.GetEnvironmentVariable("PORT") ?? "5001";

        if (Environment.GetEnvironmentVariable("CACHE_TIME") != null) 
            Globals.CACHE_TIME = Convert.ToUInt32(Environment.GetEnvironmentVariable("CACHE_TIME"));
            
        if (Environment.GetEnvironmentVariable("REFRESH_TIME") != null) 
            Globals.REFRESH_TIME = Convert.ToUInt32(Environment.GetEnvironmentVariable("REFRESH_TIME"));
            
        if (Environment.GetEnvironmentVariable("THROTTLE_WAIT") != null) 
            Globals.THROTTLE_WAIT = Convert.ToInt32(Environment.GetEnvironmentVariable("THROTTLE_WAIT"));

        if (Environment.GetEnvironmentVariable("IMAGE_CACHE_PREFIX") != null)
            Globals.IMAGE_CACHE_PREFIX = Environment.GetEnvironmentVariable("IMAGE_CACHE_PREFIX");
            
        if (!Directory.Exists(Globals.WEB_ROOT)) Directory.CreateDirectory(Globals.WEB_ROOT);
        if (!Directory.Exists(Globals.IMAGE_CACHE_DIR)) Directory.CreateDirectory(Globals.IMAGE_CACHE_DIR);
            
        var host = CreateHost();

        try {
            Log.Logger.Information("Oblivion API is starting");
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