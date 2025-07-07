using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System;
using System.Threading.Tasks;

namespace DriveBuddyyy;

public partial class MainPage : ContentPage
{
    double speedLimit = 10;

    public MainPage()
    {
        InitializeComponent();
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
                    SpeedLabel.Text = $"Speed: {Math.Round(speedKmh, 2)} km/h";

                    // Center the map using current latitude and longitude
                    var mapSpan = MapSpan.FromCenterAndRadius(
                        new Location(location.Latitude, location.Longitude),
                        Distance.FromMeters(500));

                    MyMap.MoveToRegion(mapSpan);

                    if (double.TryParse(SpeedLimitEntry.Text, out speedLimit) && speedKmh > speedLimit)
                    {
                        TriggerAlert(speedKmh);
                    }
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
