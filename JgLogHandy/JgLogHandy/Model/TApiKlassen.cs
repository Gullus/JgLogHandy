using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JgLogHandy
{
    public class TAntwortRegistrierung : IAntwortRegistrierung
    {
        public string RK { get => KennwortRegistrierung; set => KennwortRegistrierung = value; }
        public string FN { get => FahrerName; set => FahrerName = value; }
        public string LF { get => ListeKennzeichen; set => ListeKennzeichen = value; }
        public ApiStatusArten ST { get => Status; set => Status = value; }
        public string IT { get => Infotext; set => Infotext = value; }

        internal string KennwortRegistrierung;
        internal string ListeKennzeichen;
        internal ApiStatusArten Status;
        internal string FahrerName;
        internal string Infotext;
    }

    public class TAntwortAnmeldung : IAntwortAnmeldung
    {
        public string SI { get => IdSession; set => IdSession = value; }
        public ApiStatusArten ST { get => Status; set => Status = value; }
        public Guid? AL { get => IdAktLieferung; set => IdAktLieferung = value; }
        public List<TApiWerk> LWerk { get => ListeWerke; set => ListeWerke = value; }
        public List<TApiLieferung> LLief { get => ListeLieferungen; set => ListeLieferungen = value; }
        public string IT { get => Infotext; set => Infotext = value; }
        public int MF { get => Modify; set => Modify = value; }
        public string VA { get; set; } = "1.0.0";

        internal string IdSession;
        internal Guid? IdAktLieferung;
        internal List<TApiWerk> ListeWerke;
        internal List<TApiLieferung> ListeLieferungen;
        internal string Infotext;
        internal ApiStatusArten Status;
        internal int Modify = 0;
    }

    public class TApiLieferung : IApiLieferung,  INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    
        public Guid IL { get; set; }
        public string NR { get; set; }
        public string Bst { get; set; }
        public string Ort { get; set; }
        public string Str { get; set; }
        public DateTime ZB { get; set; }
        public DateTime ZP { get; set; }
        public int ZA { get; set; }
        public float ME { get; set; }
        public string AR { get; set; }
        public string EH { get; set; }
        public string KU { get; set; }
        public Guid IW { get; set; }
        public LieferArten LA { get; set; }
        public StatusLieferung ST { get; set; }
        public double? BL { get; set; }
        public double? BB { get; set; }
        public int NS { get; set; }
 
        public string Werk { get; set; }
        public string MengeAnzeige => ME.ToString("N3") + " " + EH;
        public string ZeitBaustelle
        {
            get {
                if (ZP == null)
                    return null;
                else
                    return ZP.AddMinutes(ZA).ToString("dd.MM HH:mm");
            } 
        }

        public void SetzeNeueAbweichung(int abweichung)
        {
            ZA = abweichung;
            SetProperty(nameof(ZeitBaustelle));
        }
    }

    public class TApiWerk : IApiWerk
    {
        public Guid ID { get; set; }
        public string WN { get; set; }
        public double? BL { get; set; }
        public double? BB { get; set; }
        public DateTime? GZ { get; set; } = null;
    }

    public class TAntwortLieferungen : IAntwortLieferungen
    {
        public List<TApiLieferung> LLief { get => ListeLieferung; set => ListeLieferung = value; }
        public ApiStatusArten ST { get => ApiStatus; set => ApiStatus = value; }
        public int MF { get => Modify; set => Modify = value; }

        internal List<TApiLieferung> ListeLieferung;
        internal ApiStatusArten ApiStatus = ApiStatusArten.DB;
        internal int Modify = 0;
    }

    public class TSetZusatzInfo : ISetZusatzInfo
    {
        public string SI { get => IdSession; set => IdSession = value; }
        public Guid IL { get => IdLieferung; set => IdLieferung = value; }
        public int ZM { get => AbweichungMinutenVonPlanung; set => AbweichungMinutenVonPlanung = value; }
        public string LfsN { get => LfsNummer; set => LfsNummer = value; }
        public float? LfsM { get => LfsMenge; set => LfsMenge = value; }

        internal string IdSession;
        internal Guid IdLieferung;
        internal int AbweichungMinutenVonPlanung;
        internal string LfsNummer;
        internal float? LfsMenge;
    }

    public class TAntwortZusatzInfo : IAntwortZusatzInfo
    {
        public string PN { get; set; }
        public string PH { get; set; }
        public string PT { get; set; }
        public int ZM { get => AbwMinuten; set => AbwMinuten = value; }
        public string LfsN { get; set; }
        public float? LfsM { get; set; }
        public ApiStatusArten ST { get => ApiStatus; set => ApiStatus = value; }

        [JsonIgnore]
        public string LfsMenge
        {
            get => LfsM.ToString().Replace(",", ".");
            set {
                if (string.IsNullOrWhiteSpace(value))
                    LfsM = null;
                else
                {
                    try
                    {
                        LfsM = Convert.ToSingle(value.Replace(".", ","));
                    }
                    catch { }
                }
            }
        }

        internal int AbwMinuten;
        internal ApiStatusArten ApiStatus;

        [JsonIgnore]
        public DateTime BaustDatum { get; set; }
        [JsonIgnore]
        public TimeSpan BaustZeit { get; set; }

        private DateTime _ZeitPlanung;
        [JsonIgnore]
        internal int NeueAbweichung => (int) Math.Round((BaustDatum.Date.AddSeconds(BaustZeit.TotalSeconds) - _ZeitPlanung).TotalMinutes, MidpointRounding.ToEven); 
 
        public void Init(DateTime zeitPlanung)
        {
            _ZeitPlanung = zeitPlanung;
            var datBaust = _ZeitPlanung.AddMinutes(ZM);

            BaustDatum = datBaust;
            BaustZeit = new TimeSpan(datBaust.Hour, datBaust.Minute, 0);
        }
    }

    public class TApiAction : IApiAction
    {
        public KfzActions AC { get; set; }
        public DateTime AZ { get; set; }
        public double? BL { get; set; }
        public double? BB { get; set; }

        public Guid? IL { get; set; }
        public int? NS { get; set; }
    }

    public class TSetErgebnisse : ISetAction
    {
        public string KK { get => KfzKennzeichen; set => KfzKennzeichen = value; }
        public List<TApiAction> LA { get => ListeActionen; set => ListeActionen = value; }
        public KfzActions KA { get => AktuellerKfzStatus; set => AktuellerKfzStatus = value; }
        public Guid? AL { get => IdAktLieferschein; set => IdAktLieferschein = value; }

        internal string KfzKennzeichen;
        internal List<TApiAction> ListeActionen = null;
        internal KfzActions AktuellerKfzStatus = KfzActions.Anmeldung;
        internal Guid? IdAktLieferschein = null;
    }

    public class TAntwortAction : IApiStatus
    {
        public ApiStatusArten ST { get => ApiStatus; set => ApiStatus = value; }
        internal ApiStatusArten ApiStatus = ApiStatusArten.Ok;
    }

    public class TAntwortInfoString : IAntwortInfoString
    {
        public string INF { get; set; }
        public ApiStatusArten ST { get => ApiStatus; set => ApiStatus = value; }

        internal ApiStatusArten ApiStatus;
    }

    public class TApiGps : IApiGps
    {
        public double? BL { get; set; }
        public double? BB { get; set; }
        public DateTime? GZ { get; set; }
    }
}

