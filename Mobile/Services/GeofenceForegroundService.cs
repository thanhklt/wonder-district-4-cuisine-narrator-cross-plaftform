using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using CommunityToolkit.Mvvm.Messaging;
using Mobile.Messages;
using MauiLocation = Microsoft.Maui.Devices.Sensors.Location;

namespace Mobile.Services;

[Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeLocation, Exported = false)]
public class GeofenceForegroundService : Service
{
    private const int NotificationId = 1001;
    private const string ChannelId = "geofence_channel";

    private GeofenceService? _geofence;
    private Android.Locations.ILocationListener? _locationListener;
    private Android.Locations.LocationManager? _locationManager;

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        try
        {
            CreateNotificationChannel();
            StartForeground(NotificationId, BuildNotification());

            if (_geofence is not null)
                return StartCommandResult.Sticky;

            var app = IPlatformApplication.Current;
            if (app is null) return StartCommandResult.Sticky;

            var db = app.Services.GetRequiredService<DatabaseService>();
            var audio = app.Services.GetRequiredService<AudioService>();
            _geofence = new GeofenceService(db, audio);

            _geofence.GeofenceTriggered += poi =>
                WeakReferenceMessenger.Default.Send(new GeofenceTriggeredMessage(poi.PoiID, poi.PoiName));

            StartLocationUpdates();
            _geofence.Start();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GeofenceService] OnStartCommand failed: {ex}");
        }

        return StartCommandResult.Sticky;
    }

    private void StartLocationUpdates()
    {
        _locationManager = (Android.Locations.LocationManager?)GetSystemService(LocationService);
        if (_locationManager is null) return;

        _locationListener = new SimpleLocationListener(loc =>
        {
            var accuracy = loc.HasAccuracy ? (double)loc.Accuracy : 999.0;
            var mauiLoc = new MauiLocation(loc.Latitude, loc.Longitude) { Accuracy = accuracy };
            _geofence?.UpdateLocation(mauiLoc);
            WeakReferenceMessenger.Default.Send(new Mobile.Messages.LocationUpdatedMessage(loc.Latitude, loc.Longitude));
        });

        try
        {
            _locationManager.RequestLocationUpdates(
                Android.Locations.LocationManager.GpsProvider,
                5000, 0, _locationListener);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cannot request GPS updates: {ex}");
            StopSelf();
        }
    }

    public override void OnDestroy()
    {
        _geofence?.Stop();
        if (_locationManager is not null && _locationListener is not null)
            _locationManager.RemoveUpdates(_locationListener);
        base.OnDestroy();
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;
        var channel = new NotificationChannel(ChannelId,
            "GPS Tracking", NotificationImportance.Low)
        {
            Description = "AudioTravelling theo dõi vị trí để phát audio tự động"
        };
        var nm = (NotificationManager?)GetSystemService(NotificationService);
        nm?.CreateNotificationChannel(channel);
    }

    private Notification BuildNotification() =>
        new NotificationCompat.Builder(this, ChannelId)
            .SetContentTitle("AudioTravelling")
            .SetContentText("Đang theo dõi vị trí để phát audio tự động")
            .SetSmallIcon(Android.Resource.Drawable.IcMenuMyLocation)
            .SetOngoing(true)
            .Build();
}

internal class SimpleLocationListener : Java.Lang.Object, Android.Locations.ILocationListener
{
    private readonly Action<Android.Locations.Location> _onLocation;
    public SimpleLocationListener(Action<Android.Locations.Location> onLocation) => _onLocation = onLocation;
    public void OnLocationChanged(Android.Locations.Location location) => _onLocation(location);
    public void OnProviderDisabled(string provider) { }
    public void OnProviderEnabled(string provider) { }
    public void OnStatusChanged(string? provider, Android.Locations.Availability status, Bundle? extras) { }
}
