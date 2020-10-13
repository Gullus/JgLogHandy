using Android;
using Android.App;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;

namespace JgLogHandy.Droid
{
    [Activity(Label = "JgLogHandy", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private const int RC_REQUEST_LOCATION_PERMISSION = 1000;
        private static readonly string[] REQUIRED_PERMISSIONS = { Manifest.Permission.AccessFineLocation };

        private AppOptionen _AppOptionen;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            _AppOptionen = new AppOptionen();
            _AppOptionen.ApiClientHandler = new CustomAndroidClientHandler();
            
            LoadApplication(new App(_AppOptionen));

            AppLocation.Current.LocationServiceConnected += (sender, e) =>
            {
                AppLocation.Current.LocationService.LocationChanged += _AppOptionen.Standort.StandortChange;
                AppLocation.Current.LocationService.ProviderEnabled += _AppOptionen.Standort.GpsEnabledChanged;
            };
            _AppOptionen.Standort.StartService = () =>
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == (int)Permission.Granted)
                    AppLocation.StartLocationService();
                else
                    RequestLocationPermission();
            };
            _AppOptionen.Standort.StopService = () =>
            {
                AppLocation.StopLocationService();

            };
            _AppOptionen.Standort.FuncIstGpsOn = () =>
            {
                var gpsLoc = (LocationManager)GetSystemService(LocationService);
                return gpsLoc.IsProviderEnabled("gps");
            };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AppLocation.StopLocationService();
        }

        void RequestLocationPermission()
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessFineLocation))
            {
                using (var dialog = new AlertDialog.Builder(this))
                {
                    dialog.SetTitle("Gps Ortung");
                    dialog.SetMessage("Zum korrekten Darstellung der Frachtdaten ist das einschalten des Gps notwendig !");
                    dialog.SetPositiveButton("Ok", delegate
                    {
                        ActivityCompat.RequestPermissions(this, REQUIRED_PERMISSIONS, RC_REQUEST_LOCATION_PERMISSION);
                    });
                    dialog.Show();
                }
            }
            else
                ActivityCompat.RequestPermissions(this, REQUIRED_PERMISSIONS, RC_REQUEST_LOCATION_PERMISSION);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            // Ist notwendig, da sonst der Scan bei XSing beim ersten Mal nicht funktioniert
            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == RC_REQUEST_LOCATION_PERMISSION)
            {
                if (grantResults.Length == 1 && grantResults[0] == Permission.Granted)
                {
                    _AppOptionen.Standort.ZugriffChanged(true);
                    AppLocation.StartLocationService();
                }
                else
                    _AppOptionen.Standort.ZugriffChanged(false);
            }
            else
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}