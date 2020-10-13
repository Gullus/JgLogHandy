using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JgLogHandy.Seiten
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StatusLieferungPage : ContentPage
    {
        private readonly AppOptionen _AppOptionen;

        public StatusLieferungPage(AppOptionen appOptionen)
        {
            InitializeComponent();
            _AppOptionen = appOptionen;
            this.Appearing += (sender, e) => _AppOptionen.XamPage = this;

            gridLieferung.BindingContext = _AppOptionen.Daten.AktLieferung;
        }

        private async void BtnLieferung_Clicked(object sender, EventArgs e)
        {
            StatusLieferung status = StatusLieferung.Angenommen;

            if (sender == BtnAngenommen) status = StatusLieferung.Angenommen;
            else if (sender == BtnBeladung) status = StatusLieferung.Beladung;
            else if (sender == BtnAnfahrt) status = StatusLieferung.Anfahrt;
            else if (sender == BtnAnkunft) status = StatusLieferung.Ankunft;
            else if (sender == BtnEntladung) status = StatusLieferung.Entladung;
            else if (sender == BtnFertig) status = StatusLieferung.Fertig;

            await _AppOptionen.SetzeStatusLieferung(status);
            await Navigation.PopAsync(true);
        }

        private async void BtnAbbrechen_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(true);
        }

        private async void BtnAnullieren_Clicked(object sender, EventArgs e)
        {
            _AppOptionen.Daten.AktLieferung.ST = StatusLieferung.Offen;
            await _AppOptionen.SetAction(KfzActions.Annullieren, _AppOptionen.Daten.AktLieferung.IL);
            
            _AppOptionen.Daten.AktLieferung = null;
            _AppOptionen.Daten.AktKfzAction = KfzActions.Anmeldung;
            _AppOptionen.Daten.OnLieferungChange();

            await Navigation.PopAsync(true);
        }
    }
}