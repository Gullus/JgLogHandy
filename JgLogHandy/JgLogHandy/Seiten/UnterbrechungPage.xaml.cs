using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JgLogHandy.Seiten
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UnterbrechungPage : ContentPage
    {
        private readonly AppOptionen _AppOptionen;
        private readonly KfzActions _MerkeAction;

        public UnterbrechungPage(AppOptionen appOptionen, KfzActions merkeAction)
        {
            InitializeComponent();
            _MerkeAction = merkeAction;

            _AppOptionen = appOptionen;
            this.Appearing += (sender, e) => _AppOptionen.XamPage = this;

            var txt = _AppOptionen.Daten.AktKfzAction switch 
            {
                KfzActions.Reparatur => "Reparatur",
                KfzActions.Pause => "Pause",
                _ => "Fehler !"
            };

            TxtStatus.Text = txt;
            BtnBeenden.Text = txt + " Beenden";

            gridUnterbrechung.BindingContext = new TUnterBrechung();
        }

        private async void Beenden_Clicked(object sender, EventArgs e)
        {
            await _AppOptionen.SetAction(_MerkeAction);
            await Navigation.PopModalAsync(true);
        }
    }
}