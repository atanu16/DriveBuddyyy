using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace DriveBuddyyy;

public partial class MainPage : ContentPage
{
    //initialize the variables
    double speedLimit = 10;
    DateTime lastAlertTime = DateTime.MinValue;
    private List<Location> routePoints = new(); 
    private Polyline routeLine = new(); 
    public MainPage()
    {
        InitializeComponent();
        routeLine.StrokeColor = Colors.Blue;
        routeLine.StrokeWidth = 5;
        MyMap.MapElements.Add(routeLine);
        StartMonitoringSpeed();
    }

    private async void StartMonitoringSpeed() //function to monitor the speed of the vehicle
    {
        while (true)
        {
            try
            {
                // Referenbce:- Davidortinau. (n.d.-a). Geolocation - .NET MAUI. Microsoft Learn. https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device/geolocation?view=net-maui-9.0&tabs=windows 
                var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5)));

                if (location != null)
                {
                    double speedKmh = (location.Speed ?? 0) * 3.6; // convert speed from m/s to km/hr
                    SpeedLabel.Text = $"{Math.Round(speedKmh)}";


                    //Reference: Davidortinau. (n.d.). Map - .NET MAUI. Microsoft Learn. https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/map?view=net-maui-9.0
                    // Center the map using current latitude and longitude
                    var mapSpan = MapSpan.FromCenterAndRadius(
                        new Location(location.Latitude, location.Longitude),
                        Distance.FromMeters(500));

                    MyMap.MoveToRegion(mapSpan);

                    if (double.TryParse(SpeedLimitEntry.Text, out speedLimit) && speedKmh > speedLimit)
                    {
                        TriggerAlert(speedKmh);
                    }

                    // Add point to trail
                    routePoints.Add(new Location(location.Latitude, location.Longitude));

                    // method to add polyline
                    routeLine.Geopath.Clear();
                    foreach (var point in routePoints)
                    {
                        routeLine.Geopath.Add(point);
                    }
                }
            }
            catch
            {
                SpeedLabel.Text = "⚠️ Unable to access GPS."; // catch block for error handling
            }

            await Task.Delay(3000);
        }
    }

    private void TriggerAlert(double currentSpeed) // function to send over-speeding alert message
    {
        if ((DateTime.Now - lastAlertTime).TotalSeconds < 10)
            return;

        lastAlertTime = DateTime.Now;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // Reference Davidortinau. (n.d.- a).Display pop - ups - .NET MAUI.Microsoft Learn. https://learn.microsoft.com/en-us/dotnet/maui/user-interface/pop-ups?view=net-maui-9.0
            await DisplayAlert("⚠️ Overspeeding Alert!", $"Speed: {Math.Round(currentSpeed, 1)} km/h", "OK"); // method to display alert message

            try
            {
                // Reference :- Davidortinau. (n.d.-d). Vibration - .NET MAUI. Microsoft Learn. https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device/vibrate?view=net-maui-9.0&tabs=windows
                Vibration.Default.Vibrate(TimeSpan.FromSeconds(1)); // vibrate method when alert trigger
            }
            catch (FeatureNotSupportedException)
            {
                // Device doesn't support vibration
            }
        });
    }
}
