using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System;
using System.Threading.Tasks;
using System.Collections.Generic; //--------1

namespace DriveBuddyyy;

public partial class MainPage : ContentPage
{
    double speedLimit = 10;
    DateTime lastAlertTime = DateTime.MinValue;
    private List<Location> routePoints = new(); //------2
    private Polyline routeLine = new(); //------2
    public MainPage()
    {
        InitializeComponent();
        //-----3
        routeLine.StrokeColor = Colors.Blue;
        routeLine.StrokeWidth = 5;
        MyMap.MapElements.Add(routeLine);
        //-----3
        StartMonitoringSpeed();
    }

    private async void StartMonitoringSpeed()
    {
        while (true)
        {
            try
            {
                var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5)));

                if (location != null)
                {
                    double speedKmh = (location.Speed ?? 0) * 3.6;
                    SpeedLabel.Text = $"{Math.Round(speedKmh, 2)}";

                    // Center the map using current latitude and longitude
                    var mapSpan = MapSpan.FromCenterAndRadius(
                        new Location(location.Latitude, location.Longitude),
                        Distance.FromMeters(500));

                    MyMap.MoveToRegion(mapSpan);

                    if (double.TryParse(SpeedLimitEntry.Text, out speedLimit) && speedKmh > speedLimit)
                    {
                        TriggerAlert(speedKmh);
                    }

                    // Add point to trail--4
                    routePoints.Add(new Location(location.Latitude, location.Longitude));

                    // ➕ Update polyline
                    routeLine.Geopath.Clear();
                    foreach (var point in routePoints)
                    {
                        routeLine.Geopath.Add(point);
                    }
                    //-----4
                }
            }
            catch
            {
                SpeedLabel.Text = "⚠️ Unable to access GPS.";
            }

            await Task.Delay(3000);
        }
    }

    private void TriggerAlert(double currentSpeed)
    {
        if ((DateTime.Now - lastAlertTime).TotalSeconds < 10)
            return;

        lastAlertTime = DateTime.Now;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await DisplayAlert("⚠️ Overspeeding Alert!", $"Speed: {Math.Round(currentSpeed, 1)} km/h", "OK");

            try
            {
                Vibration.Default.Vibrate(TimeSpan.FromSeconds(1));
            }
            catch (FeatureNotSupportedException)
            {
                // Device doesn't support vibration
            }
        });
    }
}
