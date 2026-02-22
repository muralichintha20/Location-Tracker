using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace LocationHeatMap;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _vm;

    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;

        // Default view (will auto move to last point when data exists)
        HeatMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(37.3349, -122.0090), Distance.FromKilometers(3)));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _vm.InitializeAsync();
        RenderHeat();

        _vm.HeatElements.CollectionChanged += (_, __) => RenderHeat();
    }

    private void RenderHeat()
    {
        HeatMap.MapElements.Clear();

        foreach (var el in _vm.HeatElements)
            HeatMap.MapElements.Add(el);

        // Auto zoom to latest point
        if (_vm.LastLocation != null)
        {
            HeatMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(_vm.LastLocation.Latitude, _vm.LastLocation.Longitude),
                Distance.FromKilometers(2)));
        }
    }
}