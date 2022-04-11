/*
 *  OblivionAPI :: MonitorService
 *
 *  This service is used to periodically update the database.
 * 
 */

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OblivionAPI.Config;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Timer = System.Timers.Timer;

namespace OblivionAPI.Services {
    public class MonitorService : IHostedService {
        private readonly ILogger<MonitorService> _logger;
        private readonly DatabaseService _database;
        private readonly Timer _timer;

        public MonitorService(ILogger<MonitorService> logger, DatabaseService database) {
            _logger = logger;
            _database = database;
            _timer = new Timer(Globals.REFRESH_TIME);
            _timer.Elapsed += UpdateOblivion;
        }

        public Task StartAsync(CancellationToken stoppingToken) {
            Task.Run(() => UpdateOblivion(this, null), stoppingToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken) {
            _timer.Stop();
            return Task.CompletedTask;
        }

        private async void UpdateOblivion(object sender, ElapsedEventArgs args) {
            _timer.Stop();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _logger.LogInformation("Oblivion monitor is running");

            try {
                await _database.HandleUpdate();
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured while updating Oblivion details");
            }
            
            stopwatch.Stop();
            _logger.LogInformation("Oblivion update completed in {Seconds} seconds", stopwatch.Elapsed.TotalSeconds);
            
            _timer.Start();
        }
    }
}
