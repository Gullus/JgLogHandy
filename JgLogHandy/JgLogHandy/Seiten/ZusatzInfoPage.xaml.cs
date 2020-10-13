using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JgLogHandy.Seiten
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ZusatzInfoPage : ContentPage
    {
        private readonly AppOptionen _AppOptionen;

        public ZusatzInfoPage(AppOptionen appOptionen, TAntwortZusatzInfo info)
        {
            InitializeComponent();
            _AppOptionen = appOptionen;
            this.Appearing += (sender, e) => _AppOptionen.XamPage = this;

            txtEinheit.Text = appOptionen.Daten.AktLieferung.EH;

            info.Init(appOptionen.Daten.AktLieferung.ZP);

            gridLieferung1.BindingContext = _AppOptionen.Daten.AktLieferung;
            gridLieferung2.BindingContext = _AppOptionen.Daten.AktLieferung;
            gridZusatzInfo1.BindingContext = info;
            gridZusatzInfo2.BindingContext = info;
        }

        private async void BtnOk_Clicked(object sender, EventArgs e)
        {
            var info = (TAntwortZusatzInfo)gridZusatzInfo2.BindingContext;
            var sendErg = new TSetZusatzInfo()
            {
                IdSession = _AppOptionen.Daten.IdSession,
                IdLieferung = _AppOptionen.Daten.AktLieferung.IL,
                AbweichungMinutenVonPlanung = info.NeueAbweichung,
                LfsNummer = info.LfsN,
                LfsMenge = info.LfsM
            };

            var (EmpfDaten, FehlerText) = await _AppOptionen.SendDaten("SetZusatzInfo/" + JsonConvert.SerializeObject(sendErg));

            if (EmpfDaten == null)
                _AppOptionen.AnzeigeDialog(DialogArten.Fehler, $"Fehler bei der Übertragung: {FehlerText} Versuchen Sie es bitte später noch einmal.");
            else
            {
                var antw = JsonConvert.DeserializeObject<TAntwortAction>(EmpfDaten);
                if (antw.ApiStatus == ApiStatusArten.Ok)
                {
                    _AppOptionen.Daten.AktLieferung.SetzeNeueAbweichung(info.NeueAbweichung);
                    await Navigation.PopModalAsync(true);
                }
                else
                    _AppOptionen.AnzeigeDialog(DialogArten.Fehler, Helper.StatusFehlerAnzeige(antw.ApiStatus));
            }
        }

        private async void BtnAbbrechen_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync(true);
        }
    }
}