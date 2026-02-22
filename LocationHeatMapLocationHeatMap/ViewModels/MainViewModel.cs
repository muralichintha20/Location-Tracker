using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LocationHeatMap.Models;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace LocationHeatMap;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly LocationRepository _repo;
    private readonly LocationTracker _tracker;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<MapElement> HeatElements { get; } = new();

    private bool _isTracking;
    public bool IsTracking
    {
        get => _isTracking;
        set
        {
            _isTracking = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TrackButtonText));
        }
    }

    public string TrackButtonText => IsTracking ? "Stop Tracking" : "Start Tracking";

    private string _status = "Ready";
    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public Location? LastLocation { get; private set; }

    public string LastPointText =>
        LastLocation == null
            ? "Last point: none"
            : $"Last point: {LastLocation.Latitude:F6}, {LastLocation.Longitude:F6}";

    public Command ToggleTrackingCommand { get; }
    public Command RefreshCommand { get; }
    public Command ClearCommand { get; }

    public MainViewModel(LocationRepository repo, LocationTracker tracker)
    {
        _repo = repo;
        _tracker = tracker;

        ToggleTrackingCommand = new Command(async () => await ToggleTrackingAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());
        ClearCommand = new Command(async () => await ClearAsync());
    }

    public async Task InitializeAsync()
    {
        await _repo.InitAsync();
        await RefreshAsync();
    }

    private async Task ToggleTrackingAsync()
    {
        if (!IsTracking)
        {
            Status = "Requesting location permission...";
            var ok = await _tracker.EnsurePermissionsAsync();
            if (!ok)
            {
                Status = "Permission denied.";
                return;
            }

            await _tracker.StartAsync();
            IsTracking = true;
            Status = "Tracking started. Wait 15-30 seconds, then tap Refresh.";
        }
        else
        {
            await _tracker.StopAsync();
            IsTracking = false;
            Status = "Tracking stopped.";
        }
    }

    private async Task RefreshAsync()
    {
        await _repo.InitAsync();
        var all = await _repo.GetAllAsync();

        if (all.Count > 0)
        {
            var last = all[^1];
            LastLocation = new Location(last.Latitude, last.Longitude);
            OnPropertyChanged(nameof(LastPointText));
        }

        BuildHeatMap(all);
        Status = $"Loaded {all.Count} point(s).";
    }

    private async Task ClearAsync()
    {
        await _repo.InitAsync();
        await _repo.ClearAsync();

        HeatElements.Clear();
        LastLocation = null;
        OnPropertyChanged(nameof(LastPointText));
        Status = "Cleared saved points.";
    }

    private void BuildHeatMap(List<LocationPoint> points)
    {
        HeatElements.Clear();
        if (points.Count == 0) return;

        // Cluster points into buckets to make heat “spots”
        var gridSize = 0.0015; // approx 150m (varies by latitude)
        var buckets = new Dictionary<(int gx, int gy), List<LocationPoint>>();

        foreach (var p in points)
        {
            var gx = (int)Math.Floor(p.Latitude / gridSize);
            var gy = (int)Math.Floor(p.Longitude / gridSize);

            var key = (gx, gy);
            if (!buckets.TryGetValue(key, out var list))
            {
                list = new List<LocationPoint>();
                buckets[key] = list;
            }
            list.Add(p);
        }

        var maxCount = buckets.Max(b => b.Value.Count);

        foreach (var bucket in buckets.Values)
        {
            var lat = bucket.Average(x => x.Latitude);
            var lon = bucket.Average(x => x.Longitude);

            var intensity = (double)bucket.Count / maxCount; // 0..1
            var radiusMeters = 40 + 180 * intensity;

            var baseColor = HeatColor(intensity);
            var alpha = 0.20 + 0.55 * intensity;

            HeatElements.Add(new Circle
            {
                Center = new Location(lat, lon),
                Radius = new Distance(radiusMeters),
                StrokeColor = Colors.Transparent,
                FillColor = baseColor.WithAlpha((float)alpha)
            });
        }
    }

    private static Color HeatColor(double t)
    {
        t = Math.Clamp(t, 0, 1);

        if (t < 0.33)
        {
            var u = t / 0.33;
            return Lerp(Colors.Blue, Colors.Green, u);
        }

        if (t < 0.66)
        {
            var u = (t - 0.33) / 0.33;
            return Lerp(Colors.Green, Colors.Yellow, u);
        }

        {
            var u = (t - 0.66) / 0.34;
            return Lerp(Colors.Yellow, Colors.Red, u);
        }
    }

    private static Color Lerp(Color a, Color b, double t)
    {
        t = Math.Clamp(t, 0, 1);
        return new Color(
            (float)(a.Red + (b.Red - a.Red) * t),
            (float)(a.Green + (b.Green - a.Green) * t),
            (float)(a.Blue + (b.Blue - a.Blue) * t),
            1f
        );
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}