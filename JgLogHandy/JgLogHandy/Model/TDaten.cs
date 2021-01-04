using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace JgLogHandy
{
    public class TDaten : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public delegate void OnLieferungChangeDelegate();
        public OnLieferungChangeDelegate OnLieferungChange { get; set; }

        internal string KennwortUser { get; set; } = null;
        internal string IdSession { get; set; } = null;

        private string _ListeKfzAsString = null;
        internal string ListeKfzAsString
        {
            get => _ListeKfzAsString;
            set
            {
                _ListeKfzAsString = value;

                AnzeigeListeKfz.Clear();
                if (_ListeKfzAsString != null)
                {
                    var listeKfz = _ListeKfzAsString.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
                    Array.Sort(listeKfz);

                    foreach (var ds in listeKfz)
                        AnzeigeListeKfz.Add(new TAnzeigeKfzInListe(ds));
                }
            }
        }

        public ObservableCollection<TAnzeigeKfzInListe> AnzeigeListeKfz { get; set; } = new ObservableCollection<TAnzeigeKfzInListe>();

        private string _FahrerName = null;
        public string FahrerName
        {
            get => _FahrerName;
            set
            {
                _FahrerName = value;
                SetProperty();
            }
        }

        internal List<TApiWerk> LWerke { get; set; }
        internal List<TApiLieferung> LLieferungen { get; set; }
        internal List<TApiAction> SendActions { get; set; } = new List<TApiAction>();

        public string AktKfz { get; set; }

        private KfzActions _KfzAction = KfzActions.Abmeldung;
        internal int LiefModify = 0;

        public KfzActions AktKfzAction
        {
            get => _KfzAction;
            set
            {
                _KfzAction = value;
                SetProperty(nameof(BtnActionText));
                SetProperty(nameof(TextAnzeigeStatus));
            }
        }

        public bool LfsGebunden => AktLieferung != null;

        public string BtnActionText
        {
            get
            {
                if (AktLieferung == null)
                    return "neue Lieferung";
                else
                    return AktLieferung.ST switch
                    {
                        StatusLieferung.Angenommen => "Beginne mit Beladung",
                        StatusLieferung.Beladung => "Anfahrt zur Baustelle",
                        StatusLieferung.Anfahrt => "Ankunft auf Baustelle",
                        StatusLieferung.Ankunft => "Beginne mit Entladung",
                        StatusLieferung.Entladung => "Entladung beendet",
                        StatusLieferung.Offen => "Lieferung annehmen",
                        StatusLieferung.Fertig => "Lieferung annehmen",
                        _ => AktLieferung.ST.ToString()
                    };
            }
        }

        public string TextAnzeigeStatus
        {
            get
            {
                if (AktLieferung == null)
                    return "keine Lieferung ausgewählt.";
                else
                {
                    return AktLieferung.ST switch
                    {
                        StatusLieferung.Angenommen => "Lieferung angenommen.",
                        StatusLieferung.Beladung => "Fahrzeug wird beladen.",
                        StatusLieferung.Anfahrt => "Anfahrt zur Baustelle.",
                        StatusLieferung.Ankunft => "Warten auf Entladung.",
                        StatusLieferung.Entladung => "Fahrzeug wird entladen.",
                        StatusLieferung.Offen => "Lieferung offen.",
                        StatusLieferung.Fertig => "Lieferung fertig.",
                        _ => "Bitte warten!"
                    };
                }
            }
        }

        internal TApiLieferung AktLieferung { get; set; } = null;

        public void Laden()
        {
            if (Application.Current.Properties.ContainsKey("KennwortRegistrierung"))
                KennwortUser = Application.Current.Properties["KennwortRegistrierung"].ToString();
            if (Application.Current.Properties.ContainsKey("FahrerName"))
                FahrerName = Application.Current.Properties["FahrerName"].ToString();
            if (Application.Current.Properties.ContainsKey("ListeKfz"))
                ListeKfzAsString = Application.Current.Properties["ListeKfz"].ToString();
        }

        public void Speichern()
        {
            Application.Current.Properties["KennwortRegistrierung"] = KennwortUser;
            Application.Current.Properties["FahrerName"] = FahrerName;
            Application.Current.Properties["ListeKfz"] = ListeKfzAsString;

            Application.Current.SavePropertiesAsync();
        }

        public void SpeichernKfz()
        {
            Application.Current.Properties["ListeKfz"] = ListeKfzAsString;
            Application.Current.SavePropertiesAsync();
        }

        public void AktualisiereLieferung()
        {
            SetProperty(nameof(BtnActionText));
            SetProperty(nameof(TextAnzeigeStatus));
            SetProperty(nameof(LfsGebunden));
        }
    }
}
