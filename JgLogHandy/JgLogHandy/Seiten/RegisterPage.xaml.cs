using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;

namespace JgLogHandy.Seiten
{
    [DesignTimeVisible(false)]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPage : ContentPage
    {
        private readonly AppOptionen _AppOptionen;

        public RegisterPage(AppOptionen appOptionen)
        {
            InitializeComponent();
            _AppOptionen = appOptionen;
            this.Appearing += (sender, e) => _AppOptionen.XamPage = this;
        }

        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        //{
        //    global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //}

        private async void BtnScan_Clicked(object sender, EventArgs e)
        {

            var overlay = new ZXingDefaultOverlay
            {
                ShowFlashButton = false,
                TopText = "Scannen sie den Code vom Bildschirm des JgLog Programms. Bei erfolgreichem Scann wird Ihr Handy automatisch initialisiert und ist sofort einsatzbereit.",
                BottomText = "Zum Abbrechen drücken Sie die Rücktaste.",
            };
            overlay.BindingContext = overlay;

            var opt = new MobileBarcodeScanningOptions()
            {
                PossibleFormats = new List<ZXing.BarcodeFormat> {
                    ZXing.BarcodeFormat.QR_CODE
                }
            };

            var scan = new ZXingScannerPage(opt, overlay) 
            { 
                Title = "Registrierung Scannen"
            };
            scan.OnScanResult += (erg) =>
            {
                scan.IsScanning = false;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    _ = await Navigation.PopAsync();
                    await _AppOptionen.HandyRegistrieren(erg.Text);
                });
            };
            await Navigation.PushAsync(scan);
        }

        private async void BtnTest_Clicked(object sender, EventArgs e)
        {
            _AppOptionen.StartWartezeichen();
            var (EmpfDaten, EmpfFehler) = await _AppOptionen.SendDaten("GetTest");
            _AppOptionen.StartWartezeichen(false);

            if (EmpfFehler != null)
                _AppOptionen.AnzeigeDialog(DialogArten.Warnung, EmpfFehler);
            else if (EmpfDaten == null)
                _AppOptionen.AnzeigeDialog(DialogArten.Warnung, "Empfangene Daten sind null");
            else
            {
                var antwort = JsonConvert.DeserializeObject<TAntwortAction>(EmpfDaten);
                _AppOptionen.AnzeigeDialog(DialogArten.Info, $"Test Server erfolgreich! Antwort ist: {antwort.ApiStatus}");
            }
        }
    }
}
