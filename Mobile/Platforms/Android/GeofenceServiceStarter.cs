using Android.Content;
using Android.OS;

namespace Mobile.Services;

public static class GeofenceServiceStarter
{
    public static async Task<bool> StartAsync(bool requestPermission)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted && requestPermission)
        {
            status = await MainThread.InvokeOnMainThreadAsync(
                Permissions.RequestAsync<Permissions.LocationWhenInUse>);
        }

        if (status != PermissionStatus.Granted)
            return false;

        try
        {
            var context = Android.App.Application.Context;
            var intent = new Intent(context, typeof(GeofenceForegroundService));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(intent);
            else
                context.StartService(intent);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cannot start geofence foreground service: {ex}");
            return false;
        }
    }
}
