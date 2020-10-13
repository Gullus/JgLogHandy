using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JgLogHandy.Seiten
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListeLieferungenPage : ContentPage
    {
        private readonly AppOptionen _AppOptionen;
        private readonly bool _NurAnzeige;

        public ListeLieferungenPage(AppOptionen appOptionen, bool nurAnzeige)
        {
            InitializeComponent();
            _AppOptionen = appOptionen;
            this.Appearing += (sender, e) => _AppOptionen.XamPage = this;

            _NurAnzeige = nurAnzeige;
            if (_NurAnzeige)
                lieferungen.BindingContext = appOptionen.Daten.LLieferungen;
            else
                lieferungen.BindingContext = appOptionen.Daten.LLieferungen
                    .Where(w => w.ST != StatusLieferung.Fertig)
                    .ToList();
        }

        private async void Lieferungen_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (!_NurAnzeige)
            {
                _AppOptionen.Daten.AktLieferung = (TApiLieferung)e.Item;
                _AppOptionen.Daten.OnLieferungChange();
                await _AppOptionen.SetzeStatusLieferung(_AppOptionen.Daten.AktLieferung.ST == StatusLieferung.Offen ? StatusLieferung.Angenommen : _AppOptionen.Daten.AktLieferung.ST);
            }            

            await Navigation.PopAsync(true);
        }
    }
}