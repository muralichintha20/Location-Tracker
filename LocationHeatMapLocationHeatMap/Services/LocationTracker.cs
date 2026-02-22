using LocationHeatMap.Models;

namespace LocationHeatMap;

public class LocationTracker
{
    private readonly LocationRepository _repo;

    private CancellationTokenSource? _cts;

    public bool IsRunning => _cts != null;
    public int IntervalSeconds { get; set; } = 5;

    public LocationTracker(LocationRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> EnsurePermissionsAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        return status == PermissionStatus.Granted;
    }

    public async Task StartAsync()
    {
        if (IsRunning) return;

        await _repo.InitAsync();
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => LoopAsync(_cts.Token));
    }

    public Task StopAsync()
    {
        _cts?.Cancel();
        _cts = null;
        return Task.CompletedTask;
    }

    private async Task LoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request, token);

                if (location != null)
                {
                    var point = new LocationPoint
                    {
                        TimestampUtc = DateTime.UtcNow,
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        AccuracyMeters = location.Accuracy ?? 0
                    };

                    await _repo.InsertAsync(point);
                }
            }
            catch
            {
                // keep looping
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(IntervalSeconds), token);
            }
            catch
            {
                // ignore
            }
        }
    }
}