using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace JgLogHandy
{
    public class AppOptionen
    {
        internal ContentPage XamPage = null;
        internal Grid GridStammLieferung = null;

        internal HttpClient ApiClient = null;
        public object ApiClientHandler = null;

#if DEBUG
        private const string _UrlServer = "http://Gullus-Laptop:5000/jglogapi/";
#else
        private const string _UrlServer = "https://jglog.dd-dns.de:6101/jglogApi/";
#endif

        public TStandort Standort { get; private set; }

        internal TDaten Daten { get; } = new TDaten();

        public AppOptionen()
        {
            Standort = new TStandort();
        }

        public void Start()
        {
            Standort.Init(this, Daten.IdSession);
        }

        internal async Task<bool> SetzeStatusLieferung(StatusLieferung statusLieferung)
        {
            Daten.AktLieferung.ST = statusLieferung;

            if (Enum.TryParse<KfzActions>(statusLieferung.ToString(), out var status))
            {
                var (ApiStatus, _) = await SetAction(status, Daten.AktLieferung.IL);
                var anzFehler = new ApiStatusArten[] { ApiStatusArten.RE, ApiStatusArten.KB, ApiStatusArten.WZ };

                if (anzFehler.Contains(ApiStatus))
                    AnzeigeDialog(DialogArten.Fehler, Helper.StatusFehlerAnzeige(ApiStatus));
            }
            else
                Daten.AktKfzAction = KfzActions.Fehler;

            if (Daten.AktLieferung.ST == StatusLieferung.Fertig)
            {
                Daten.AktLieferung = null;
                Daten.OnLieferungChange();
            }

            Daten.AktualisiereLieferung();

            return true;
        }

        internal async Task HandyRegistrieren(string scanText)
        {
            var felder = scanText.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (felder.Length != 2)
                AnzeigeDialog(DialogArten.Warnung, $"Es müssen zwei Kennwörter angegeben werden! ({scanText})");
            else
            {
                var queryDaten = new Dictionary<string, object>()
                {
                    { "KV", felder[1] } // Kennwort Verantwortlicher
                };

                StartWartezeichen();
                var (EmpfDaten, EmpfFehler) = await SendDaten("RegisterHandy", felder[0], queryDaten);  // felder[0] => Kennwort User
                StartWartezeichen(false);

                if (EmpfDaten == null)
                    AnzeigeDialog(DialogArten.Fehler, EmpfFehler);
                else
                {
                    var antwort = JsonConvert.DeserializeObject<TAntwortRegistrierung>(EmpfDaten);

                    if (antwort.Status == ApiStatusArten.RE)
                        AnzeigeDialog(DialogArten.Fehler, "Registrierung fehlerhaft! Rufen Sie die Seite mit dem QR Code im Computer erneut auf.");
                    else if (antwort.Status != ApiStatusArten.Ok)
                        AnzeigeDialog(DialogArten.Fehler, Helper.StatusFehlerAnzeige(antwort.ST));
                    else
                    {
                        Daten.ListeKfz = antwort.ListeKennzeichen;
                        Daten.FahrerName = antwort.FahrerName;
                        Daten.KennwortUser = antwort.KennwortRegistrierung;

                        Daten.Speichern();
                        await XamPage.Navigation.PopModalAsync(true);
                    }
                }
            }
        }

        internal async Task<bool> Anmeldung(string aktKfz)
        {
            Daten.AktLieferung = null;
            Daten.AktKfz = aktKfz;

            var queryDaten = new Dictionary<string, object>()
            {
                { "MK", Helper.GetKfzMatchCode(aktKfz) },
                { "ZA", DateTime.Now }
            };

            StartWartezeichen();
            var (EmpfDaten, EmpfFehler) = await SendDaten("GetAnmeldung", Daten.KennwortUser, queryDaten);
            StartWartezeichen(false);

            if (EmpfDaten == null)
                AnzeigeDialog(DialogArten.Fehler, EmpfFehler);
            else
            {
                var antwort = JsonConvert.DeserializeObject<TAntwortAnmeldung>(EmpfDaten);

                if (antwort.Status != ApiStatusArten.Ok)
                    AnzeigeDialog(DialogArten.Fehler, Helper.StatusFehlerAnzeige(antwort.Status, antwort.Infotext));
                else
                {
                    Daten.IdSession = antwort.IdSession;
                    Daten.LWerke = antwort.ListeWerke;
                    Daten.LLieferungen = antwort.ListeLieferungen;
                    Daten.LiefModify = antwort.Modify;

                    if (antwort.IdAktLieferung == null)
                        Daten.AktKfzAction = KfzActions.Anmeldung;
                    else
                    {
                        Daten.AktLieferung = Daten.LLieferungen.FirstOrDefault(f => f.IL == antwort.IdAktLieferung);
                        if ((Daten.AktLieferung != null) && (Enum.TryParse<KfzActions>(Daten.AktLieferung.ToString(), out var status)))
                            Daten.AktKfzAction = status;
                    }

                    // Werkname in Lieferung eintragen

                    WerkenameInLieferungEintragen(Daten.LLieferungen);

                    if (antwort.Infotext != null)
                        AnzeigeDialog(DialogArten.Info, antwort.Infotext);

                    return true;
                }
            }

            return false;
        }

        internal async Task<(ApiStatusArten ApiStatus, string Fehlertext)> SetAction(KfzActions kfzAction, Guid? idLieferung = null)
        {
            Daten.AktKfzAction = kfzAction;

            Daten.SendActions.Add(new TApiAction()
            {
                AC = kfzAction,
                AZ = Helper.GetDateTime(),
                IL = idLieferung,
                NS = Daten.AktLieferung?.NS,
  
                BB = Standort.Messung.BB,
                BL = Standort.Messung.BL
            });

            var sendErg = new TSetErgebnisse()
            {
                AktuellerKfzStatus = Daten.AktKfzAction,
                IdAktLieferschein = Daten.AktLieferung?.IL,
                KfzKennzeichen = Helper.GetKfzMatchCode(Daten.AktKfz),
                ListeActionen = Daten.SendActions
            };

            var (EmpfDaten, FehlerText) = await SendDaten("SetApiAction", Daten.IdSession, null, sendErg);

            if (FehlerText != null)
                return (ApiStatusArten.SendFehler, FehlerText);

            var ergEmpf = JsonConvert.DeserializeObject<TAntwortAction>(EmpfDaten);

            if (ergEmpf.ApiStatus != ApiStatusArten.DB)
                Daten.SendActions.Clear();

            return (ergEmpf.ApiStatus, Helper.StatusFehlerAnzeige(ergEmpf.ApiStatus));

        }

        internal void AnzeigeDialog(DialogArten dialogArt, string text)
        {
            XamPage.DisplayAlert(dialogArt.ToString(), text, " Ok ");
        }

        internal void StartWartezeichen(bool start = true)
        {
            var indi = XamPage.FindByName<ActivityIndicator>("indikatorHandy");
            indi.IsRunning = start;
        }

        internal void WerkenameInLieferungEintragen(List<TApiLieferung> lieferungen)
        {
            if (lieferungen == null)
                return;

            foreach (var lief in lieferungen)
            {
                var werk = Daten.LWerke.FirstOrDefault(f => f.ID == lief.IW);
                if (werk != null)
                    lief.Werk = werk.WN;
            }
        }

        internal async Task<(int AnzahlAktualisiert, string FehlerMeldung)> LieferungenAktualisieren()
        {
            var anzAktualisiert = -1;

            var queryString = new Dictionary<string, object>()
            {
                { "MF", Daten.LiefModify }
            };

            StartWartezeichen();
            var (EmpfDaten, FehlerText) = await SendDaten("GetLieferungen", Daten.IdSession, queryString);
            StartWartezeichen(false);

            if (FehlerText != null)
                return (anzAktualisiert, FehlerText);
            if (EmpfDaten == null)
                return (anzAktualisiert, $"Fehler bei der Übertragung: {FehlerText} Versuchen Sie es bitte später noch einmal.");

            var antwort = JsonConvert.DeserializeObject<TAntwortLieferungen>(EmpfDaten);

            if (antwort.ApiStatus != ApiStatusArten.Ok)
                return (anzAktualisiert, Helper.StatusFehlerAnzeige(antwort.ApiStatus));

            if (antwort.LLief.Count == 0)
                return (0, null);

            Daten.LiefModify = antwort.Modify;
            var aktLiefId = Daten.AktLieferung?.IL;

            foreach (var liefNeu in antwort.LLief)
            {
                var liefAlt = Daten.LLieferungen.FirstOrDefault(f => f.IL == liefNeu.IL);
                if (liefAlt != null)
                    Daten.LLieferungen.Remove(liefAlt);
            }

            WerkenameInLieferungEintragen(antwort.LLief);

            Daten.LLieferungen.AddRange(antwort.LLief);
            Daten.LLieferungen.Sort((x, y) => (x.ZP < y.ZP) ? -1 : 1);

            // Wenn aktuelle Lieferung geändert diese neu im Display anzeigen

            if ((aktLiefId != null) && (antwort.LLief.Any(a => a.IL == aktLiefId)))
            {
                GridStammLieferung.BindingContext = null;
                Daten.AktLieferung = Daten.LLieferungen.FirstOrDefault(f => f.IL == aktLiefId);
                GridStammLieferung.BindingContext = Daten.AktLieferung;
                Daten.AktualisiereLieferung();
            }

            return (antwort.LLief.Count, null);
        }

        public async Task<(string EmpfDaten, string EmpfFehler)> SendDaten(string url, string id = "",
            Dictionary<string, object> queryDaten = null, object bodyDaten = null)
        {
            var current = Connectivity.NetworkAccess;

            var fehler = current switch
            {
                NetworkAccess.Unknown => "Netzwerkstatus unbekannt.",
                NetworkAccess.None => "Kein Netzwerk vorhanden.",
                NetworkAccess.Local => "Nur locales Netzwerk vorhanden.",
                NetworkAccess.ConstrainedInternet => "Nur eingeschränktes Netzwerk vorhanden.",
                _ => null
            };

            if (fehler == null)
            {
                try
                {
                    var adresse = new StringBuilder($"{_UrlServer}{url}/");
                    if (!string.IsNullOrWhiteSpace(id))
                        adresse.Append(id + "?");

                    if (queryDaten != null)
                    {
                        foreach (var qd in queryDaten)
                        {
                            if (qd.Value == null)
                                adresse.Append(qd.Key + "=null");
                            else if (qd.Value is DateTime time)
                                adresse.Append(qd.Key + "=" + time.ToString("yyyy-MM-dd HH:mm:ss"));
                            else
                                adresse.Append(qd.Key + "=" + qd.Value.ToString());

                            adresse.Append("&");
                        }

                        _ = adresse.Remove(adresse.Length - 1, 1);
                    }

                    HttpResponseMessage responce;
                    if (bodyDaten == null)
                        responce = await ApiClient.GetAsync(adresse.ToString());
                    else
                        responce = await ApiClient.PostAsync(adresse.ToString(), new StringContent(JsonConvert.SerializeObject(bodyDaten), Encoding.UTF8, "application/json"));

                    if (responce.StatusCode == HttpStatusCode.OK)
                        return (await responce.Content.ReadAsStringAsync(), null);
                    else
                        fehler = $"Fehler Html Antwort: {responce.StatusCode}";
                }
                catch (OperationCanceledException ex)
                {
                    fehler = $"Zeitüberschreitung beim senden. Grund: {ex.Message}";
                }
                catch (WebException ex)
                {
                    fehler = ex.Message.ToLower().Contains("failed to connect to")
                        ? "Verbindung zum Server fehlgeschlagen!"
                        : $"Webfehler: {ex.Message}";
                }
                catch (HttpRequestException ex)
                {
                    fehler = $"Requestfehler: {ex.Message}";
                }
                catch (Exception ex)
                {
                    fehler = $"Allg. Fehler: {ex.Message}{(ex.InnerException == null ? "" : " - " + ex.InnerException.Message)} ({ex.GetType()})";
                }
            }

            return (null, fehler);
        }
    }
}
