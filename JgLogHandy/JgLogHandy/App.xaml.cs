using JgLogHandy.Seiten;
using System;
using System.Net.Http;
using Xamarin.Forms;

namespace JgLogHandy
{
    public partial class App : Application
    {
        public App(AppOptionen appOptionen)
        {
            InitializeComponent();

            appOptionen.Daten.Laden();

#if DEBUG
            appOptionen.ApiClient = new HttpClient((HttpMessageHandler)appOptionen.ApiClientHandler);
#else
            appOptionen.ApiClient = new HttpClient((HttpMessageHandler)appOptionen.ApiClientHandler) { Timeout = new TimeSpan(0, 0, 10) };
#endif

            MainPage = new NavigationPage(new AnmeldungPage(appOptionen));
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
