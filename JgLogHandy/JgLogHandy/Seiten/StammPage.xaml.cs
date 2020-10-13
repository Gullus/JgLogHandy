using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JgLogHandy.Seiten
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StammPage : ContentPage
    {
        private readonly AppOptionen _AppOptionen;

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                _AppOptionen.AnzeigeDialog(DialogArten.Info, "Benutzen Sie den Abmelde Button");
            });

            return true;
        }

        public StammPage(AppOptionen appOptionen)
        {
            InitializeComponent();

            _AppOptionen = appOptionen;
            _AppOptionen.GridStammLieferung = gridLieferung;
            this.Appearing += (sender, e) => _AppOptionen.XamPage = this;

            _AppOptionen.Daten.OnLieferungChange = () => gridLieferung.BindingContext = _AppOptionen.Daten.AktLieferung;
            _AppOptionen.Standort.AnzeigeGpsEnabledChange = (umg) =>
            {
                if (!umg)
                    DisplayAlert("Standortermittlung", "Sie haben keine Gps eingeschaltet. Dadurch ist eine optimale Planung nicht möglich.", "Ok");
            };

            gridDaten1.BindingContext = _AppOptionen.Daten;
            gridDaten2.BindingContext = _AppOptionen.Daten;
            gridLieferung.BindingContext = _AppOptionen.Daten.AktLieferung;

            _AppOptionen.Standort.Init(_AppOptionen, _AppOptionen.Daten.IdSession);
            _AppOptionen.Standort.StartGps();
        }

        private async void BtnAction_Clicked(object sender, EventArgs e)
        {
            if (_AppOptionen.Daten.AktLieferung == null)
            {
                _ = await _AppOptionen.LieferungenAktualisieren();
                await Navigation.PushAsync(new ListeLieferungenPage(_AppOptionen, false), true);
            }
            else
                await _AppOptionen.SetzeStatusLieferung((StatusLieferung)1 + (byte)_AppOptionen.Daten.AktLieferung.ST);
        }

        private void BtnStatusKorrektur_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new StatusLieferungPage(_AppOptionen), true);
        }

        private async void Unterbrechung_Clicked(object sender, EventArgs e)
        {
            string erg = await DisplayActionSheet("Grund der Unterbrechung:", "Abbrechen", null, "Pause", "Reparatur");

            if (erg != "Abbrechen")
            {
                var merkeAction = _AppOptionen.Daten.AktKfzAction;
                switch (erg)
                {
                    case "Pause": await _AppOptionen.SetAction(KfzActions.Pause); break;
                    case "Reparatur": await _AppOptionen.SetAction(KfzActions.Reparatur); break;
                }
                await Navigation.PushModalAsync(new UnterbrechungPage(_AppOptionen, merkeAction), true);
            }
        }

        private async void Abmelden_Clicked(object sender, EventArgs e)
        {
            var (ApiStatus, Fehlertext) = await _AppOptionen.SetAction(KfzActions.Abmeldung);
            if (ApiStatus == ApiStatusArten.SendFehler)
                _AppOptionen.AnzeigeDialog(DialogArten.Fehler, "Fehler bei der Abmeldung. Grund: " + Fehlertext);
            else
                _ = await Navigation.PopModalAsync(true);

            _AppOptionen.Standort.StopGps();
        }

        private async void BtnZusatzInfo_Clicked(object sender, EventArgs e)
        {
            var queryString = new Dictionary<string, object>()
            {
                { "LI", _AppOptionen.Daten.AktLieferung.IL }
            };

            _AppOptionen.StartWartezeichen();
            var (EmpfDaten, FehlerText) = await _AppOptionen.SendDaten("GetZusatzInfo", _AppOptionen.Daten.IdSession, queryString);
            _AppOptionen.StartWartezeichen(false);

            if (EmpfDaten == null)
                _AppOptionen.AnzeigeDialog(DialogArten.Fehler, $"Fehler bei der Übertragung: {FehlerText} Versuchen Sie es bitte später noch einmal.");
            {
                var antwort = JsonConvert.DeserializeObject<TAntwortZusatzInfo>(EmpfDaten);

                if (antwort.ApiStatus == ApiStatusArten.Ok)
                    await Navigation.PushModalAsync(new ZusatzInfoPage(_AppOptionen, antwort), true);
                else
                    _AppOptionen.AnzeigeDialog(DialogArten.Fehler, Helper.StatusFehlerAnzeige(antwort.ApiStatus));
            }
        }

        private async void Route_Clicked(object sender, EventArgs e)
        {
            if (_AppOptionen.Daten.AktLieferung.BB == null)
                _AppOptionen.AnzeigeDialog(DialogArten.Info, "Baustelle wurde ohne GPS Daten angelegt.");
            else
            {
                var bstBreite = _AppOptionen.Daten.AktLieferung.BB.Value.ToString(CultureInfo.InvariantCulture);
                var bstLaenge = _AppOptionen.Daten.AktLieferung.BL.Value.ToString(CultureInfo.InvariantCulture);

                var url = $"https://www.google.com/maps/dir/?api=1&destination={bstBreite},{bstLaenge}&travelmode=DRIVING";

                if (_AppOptionen.Daten.AktLieferung.ST == StatusLieferung.Angenommen)
                {
                    var werk = _AppOptionen.Daten.LWerke.First(f => f.ID == _AppOptionen.Daten.AktLieferung.IW);
                    var werkLaenge = werk.BL.Value.ToString(CultureInfo.InvariantCulture);
                    var werkBreite = werk.BB.Value.ToString(CultureInfo.InvariantCulture);
                    url = $"https://www.google.com/maps/dir/?api=1&origin&waypoints={werkBreite},{werkLaenge}&destination={bstBreite},{bstLaenge}&travelmode=DRIVING";
                }

                await Launcher.OpenAsync(url);
            }
        }

        private async void BtnUebersichtLieferungen_Clicked(object sender, EventArgs e)
        {
            _ = await _AppOptionen.LieferungenAktualisieren();
            await Navigation.PushAsync(new ListeLieferungenPage(_AppOptionen, true), true);
        }

        private async void BtnLieferungenAktualisieren_Clicked(object sender, EventArgs e)
        {
            var (AnzahlAktualisiert, FehlerMeldung) = await _AppOptionen.LieferungenAktualisieren();

            if (AnzahlAktualisiert == -1)
                _AppOptionen.AnzeigeDialog(DialogArten.Warnung, FehlerMeldung);
            else
            {
                var s = AnzahlAktualisiert == 0 ? "Keine Aktualisierungen vorhanden." : $"Es wurden {AnzahlAktualisiert} Lieferungen geändert!";
                _AppOptionen.AnzeigeDialog(DialogArten.Info, s);
            }
        }
    }
}