using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
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
                    double speedKmh = (location.Speed ?? 0) * 3.6; // Convert m/s to km/h
                    SpeedLabel.Text = $"Speed: {Math.Round(speedKmh, 2)} km/h";

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

            await Task.Delay(3000); // wait 3 seconds before next update
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
                // Vibration not supported
            }
        });
    }
}
