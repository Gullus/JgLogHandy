using Android.App;
using Android.Content;
using Android.OS;
using JgLogHandy.Droid.Services;
using System;
using System.Threading.Tasks;

namespace JgLogHandy.Droid
{
    public class AppLocation
    {
        public static AppLocation Current { get; }

        protected static LocationServiceConnection locationServiceConnection;

        static AppLocation()
        {
            Current = new AppLocation();
        }

        protected AppLocation()
        {
            // create a new service connection so we can get a binder to the service
            locationServiceConnection = new LocationServiceConnection(null);

            // this event will fire when the Service connectin in the OnServiceConnected call 
            locationServiceConnection.ServiceConnected += (sender, e) =>
                LocationServiceConnected(this, e);
        }

        public LocationService LocationService
        {
            get {
                if (locationServiceConnection.Binder == null)
                {
                    throw new Exception("Service not bound yet");
                }

                // note that we use the ServiceConnection to get the Binder, and the Binder to get the Service here
                return locationServiceConnection.Binder.Service;
            }
        }

        // events
        public event EventHandler<ServiceConnectedEventArgs> LocationServiceConnected = delegate { };

        public static void StartLocationService()
        {
            // Starting a service like this is blocking, so we want to do it on a background thread
            new Task(() =>
            {
                // Check if device is running Android 8.0 or higher and if so, use the newer StartForegroundService() method
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Application.Context.StartForegroundService(new Intent(Application.Context, typeof(LocationService)));
                }
                else // For older versions, use the traditional StartService() method
                {
                    Application.Context.StartService(new Intent(Application.Context, typeof(LocationService)));
                }

                // bind our service (Android goes and finds the running service by type, and puts a reference
                // on the binder to that service)
                // The Intent tells the OS where to find our Service (the Context) and the Type of Service
                // we're looking for (LocationService)
                var locationServiceIntent = new Intent(Application.Context, typeof(LocationService));

                // Finally, we can bind to the Service using our Intent and the ServiceConnection we
                // created in a previous step.
                Application.Context.BindService(locationServiceIntent, locationServiceConnection, Bind.AutoCreate);
            }).Start();
        }

        public static void StopLocationService()
        {
            // Unbind from the LocationService; otherwise, StopSelf (below) will not work:
            if (locationServiceConnection != null)
                Application.Context.UnbindService(locationServiceConnection);

            // Stop the LocationService:
            if (Current.LocationService != null)
                Current.LocationService.StopSelf();
        }
    }
}