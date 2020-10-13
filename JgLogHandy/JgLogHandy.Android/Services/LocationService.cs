using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Support.V4.App;

namespace JgLogHandy.Droid.Services
{
    [Service]
    public class LocationService : Service, ILocationListener
    {
        public delegate void LocationChangedDelegate(double breite, double laenge, double speed);
        public delegate void ProviderEnabledDelegate(string provider, bool isEnabled);

        public LocationChangedDelegate LocationChanged;
        public ProviderEnabledDelegate ProviderEnabled;

        const int SERVICE_RUNNING_NOTIFICATION_ID = 123;
        const string NOTIFICATION_CHANNEL_ID = "com.company.app.channel";

        IBinder binder;

        protected LocationManager LocMgr = Application.Context.GetSystemService("location") as LocationManager;

        public void OnLocationChanged(Android.Locations.Location location)
        {
            LocationChanged(location.Latitude, location.Longitude, location.Speed);
        }

        public void OnProviderDisabled(string provider)
        {
            ProviderEnabled(provider, false);
        }

        public void OnProviderEnabled(string provider)
        {
            ProviderEnabled(provider, true);
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            // Wird zur Zeit icht gebraucht
            // StatusChanged(this, new StatusChangedEventArgs(provider, status, extras));
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }

        // This gets called when StartService is called in our App class
        // [Obsolete("deprecated in base class")]
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            // Check if device is running Android 8.0 or higher and call StartForeground() if so
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notification = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID)
                                   .SetContentTitle("Jg-Logistik")
                                   .SetContentText("Logistik Programm")
                                   //.SetSmallIcon(Resource.Drawable.notification_icon_background)
                                   .SetOngoing(true)
                                   .Build();

                var notificationManager =
                    GetSystemService(NotificationService) as NotificationManager;

                var chan = new NotificationChannel(NOTIFICATION_CHANNEL_ID, "On-going Notification", NotificationImportance.Min);

                notificationManager.CreateNotificationChannel(chan);

                StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, notification);
            }

            return StartCommandResult.Sticky;
        }

        // This gets called once, the first time any client bind to the Service
        // and returns an instance of the LocationServiceBinder. All future clients will
        // reuse the same instance of the binder
        public override IBinder OnBind(Intent intent)
        {
            binder = new LocationServiceBinder(this);
            return binder;
        }

        // Handle location updates from the location manager
        public void StartLocationUpdates()
        {
            //we can set different location criteria based on requirements for our app -
            //for example, we might want to preserve power, or get extreme accuracy
            var locationCriteria = new Criteria
            {
                Accuracy = Accuracy.NoRequirement,
                PowerRequirement = Power.NoRequirement,
                AltitudeRequired = false,
                BearingRequired = false,
            };

            // get provider: GPS, Network, etc.
            var locationProvider = LocMgr.GetBestProvider(locationCriteria, true);

            // Get an initial fix on location
            if (string.IsNullOrWhiteSpace(locationProvider))
                locationProvider = "gps";

            LocMgr.RequestLocationUpdates(locationProvider, 2000, 0, this);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            LocMgr.RemoveUpdates(this);
        }
    }
}
