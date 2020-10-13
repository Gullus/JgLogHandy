using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            var auswKfz = listKfz.SelectedItem;

            if (auswKfz == null)
            {
                //DisplayAlert("Mein Titel", "Die Message", "Ok"); //  (, text, " Ok ");
                _AppOptionen.AnzeigeDialog(DialogArten.Info, "Es muss ein Fahrzeug ausgewählt werden!");
            }
            else
            {
                if (!await _AppOptionen.Anmeldung(auswKfz.ToString()))
                    return;

                await Navigation.PushModalAsync(new NavigationPage(new StammPage(_AppOptionen)));
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
                    _AppOptionen.Daten.ListeKfz = antwort.INF;
                    _AppOptionen.Daten.SpeichernKfz();
                    var kfzSource =_AppOptionen.Daten.ListeKfz.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries); 
                    listKfz.ItemsSource = kfzSource;
                    BtnAnmelden.IsEnabled = kfzSource.Length > 0;
                }
            }
        }
    
        private async void BtnClicked_Registrierung(object dender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new RegisterPage(_AppOptionen)));
        }
    }
}
