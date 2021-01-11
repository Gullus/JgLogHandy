using Newtonsoft.Json;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JgLogHandy.Seiten
{
    [DesignTimeVisible(false)]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AnmeldungPage : ContentPage
    {
        private readonly AppOptionen _AppOptionen;

        public AnmeldungPage(AppOptionen appOptionen)
        {
            InitializeComponent();
            _AppOptionen = appOptionen;

            this.Appearing += (sender, e) => _AppOptionen.XamPage = this;
            gridDaten.BindingContext = _AppOptionen.Daten;

            if (appOptionen.Daten.KennwortUser == null)
                RegisterSeiteAufrufen();
        }

        private async void RegisterSeiteAufrufen()
        {
            await Navigation.PushModalAsync(new NavigationPage(new RegisterPage(_AppOptionen)));
        }

        private async void BtnClicked_Anmeldung(object sender, EventArgs e)
        {
            if (_AppOptionen.Daten.AnzeigeListeKfz.Count == 0)
                _AppOptionen.AnzeigeDialog(DialogArten.Info, "Ihnen wurde in den Stammdaten kein Fahrzeug zugeordnet. Wenden Sie sich bitte an einen verantwortlichen Mitarbeiter!");
            else
            {
                if (listKfz.SelectedItem == null)
                    _AppOptionen.AnzeigeDialog(DialogArten.Info, "Es muss ein Fahrzeug ausgewählt werden!");
                else
                {
                    var auswKfz = (listKfz.SelectedItem as TAnzeigeKfzInListe).AnzeigeKfz;
                    if (await _AppOptionen.Anmeldung(auswKfz))
                        await Navigation.PushModalAsync(new NavigationPage(new StammPage(_AppOptionen)));
                }
            }
        }

        private async void BtnClicked_KfzAktualisieren(object sender, EventArgs e)
        {
            _AppOptionen.StartWartezeichen();
            var (EmpfDaten, FehlerText) = await _AppOptionen.SendDaten("GetListeKfz", _AppOptionen.Daten.KennwortUser);
            _AppOptionen.StartWartezeichen(false);

            if (EmpfDaten == null)
                _AppOptionen.AnzeigeDialog(DialogArten.Fehler, $"Fehler bei der Übertragung: {FehlerText} Versuchen Sie es bitte später noch einmal.");
            else
            {
                var antwort = JsonConvert.DeserializeObject<TAntwortInfoString>(EmpfDaten);

                if (antwort.ApiStatus != ApiStatusArten.Ok)
                    _AppOptionen.AnzeigeDialog(DialogArten.Fehler, Helper.StatusFehlerAnzeige(antwort.ApiStatus));
                else
                {
                    _AppOptionen.Daten.ListeKfzAsString = antwort.INF;
                    _AppOptionen.Daten.SpeichernKfz();
                }
            }
        }

        private async void BtnClicked_Registrierung(object dender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new RegisterPage(_AppOptionen)));
        }
    }
}
