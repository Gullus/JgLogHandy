using JgLogHandy;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JgLogHandy.Seiten
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoggerPage : ContentPage
    {
        public LoggerPage(AppOptionen appOptionen)
        {
            InitializeComponent();
            //gridLogger.BindingContext = appOptionen.Log.Logs;
        }
    }
}