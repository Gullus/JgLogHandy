using System;
using System.Collections.Generic;
using System.Globalization;

namespace JgLogHandy
{
    public class TStandort
    {
        private AppOptionen _AppOptionen = null;
        private string _IdSession = null;

        public Action StartService;
        public Action StopService;

        public bool IstZugriffOk { get; private set; } = true;

        public bool IstGpsOn => FuncIstGpsOn();
        public Func<bool> FuncIstGpsOn;

        public bool GpsOn { get; private set; } = false;
        public Action<bool> AnzeigeGpsEnabledChange;

        public bool WerteOk => GpsOn && IstZugriffOk && (Messung.BB != null);

        public TApiGps Messung { get; private set; } = new TApiGps();

        public void Init(AppOptionen appOptionen, string idSession)
        {
            _AppOptionen = appOptionen;
            _IdSession = idSession;
        }

        public async void StandortChange(double breite, double laenge, double speed)
        {
            //Log.Warning("Location", $"Location: {breite} - {laenge}");

            Messung.BB = breite;
            Messung.BL = laenge;
            Messung.GZ = Helper.GetDateTime();

            var queryString = new Dictionary<string, object>()
            {
                { "bb" , breite.ToString(CultureInfo.InvariantCulture) },
                { "bl" , laenge.ToString(CultureInfo.InvariantCulture) }
            };

            var (_, _) = await _AppOptionen.SendDaten("SetGps", _IdSession, queryString);
        }

        public void StartGps()
        {
            StartService();
        }

        public void StopGps()
        {
            StopService();
        }

        public void ZugriffChanged(bool istOk)
        {
            IstZugriffOk = istOk;
        }

        public void GpsEnabledChanged(string provider, bool isEnabled)
        {
            GpsOn = isEnabled;
            AnzeigeGpsEnabledChange(GpsOn);
        }
    }
}
