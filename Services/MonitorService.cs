/*
 *  OblivionAPI :: MonitorService
 *
 *  This service is used to periodically update the database.
 * 
 */

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Timers;

using Timer = System.Timers.Timer;

namespace OblivionAPI.Services; 

public class MonitorService : IHostedService {
    private readonly ILogger<MonitorService> _logger;
    private readonly DatabaseService _database;
    private readonly BlockchainService _blockchain;
    private readonly ImageCacheService _imageCache;
    private readonly LookupService _lookup;
    private readonly Timer _timer;
    private Timer _maintenanceTimer;

    public MonitorService(ILogger<MonitorService> logger, DatabaseService database, BlockchainService blockchain, ImageCacheService imageCache, LookupService lookup) {
        _logger = logger;
        _database = database;
        _blockchain = blockchain;
        _imageCache = imageCache;
        _lookup = lookup;
        _timer = new Timer(Globals.REFRESH_TIME);
        _timer.Elapsed += UpdateOblivion;
    }

    public async Task StartAsync(CancellationToken stoppingToken) {
        if (!_database.DatabaseLoaded) await _database.LoadDatabase();
        StartMaintenanceTimer();
        await Task.Run(() => UpdateOblivion(this, null), stoppingToken);
    }

    public Task StopAsync(CancellationToken stoppingToken) {
        _timer.Stop();
        return Task.CompletedTask;
    }

    private void StartMaintenanceTimer() {
        var time = DateTime.Now.TimeOfDay;
        var nextHour = TimeSpan.FromHours(Math.Ceiling(time.TotalHours));
        var delta = (nextHour - time).TotalMilliseconds;
        _maintenanceTimer = new Timer(delta);
        _maintenanceTimer.Elapsed += HourlyMaintenance;
        _maintenanceTimer.Start();
        _logger.LogDebug("Maintenance timer started with {Timer} milliseconds", delta);
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

    private void HourlyMaintenance(object sender, ElapsedEventArgs args) {
        _logger.LogInformation("Running hourly maintenance");
        _maintenanceTimer.Stop();
        
        _blockchain.ResetCounters();
        _imageCache.ResetCounters();
        _lookup.ResetCounters();
        
        StartMaintenanceTimer();
    }
}