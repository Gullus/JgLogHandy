using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace JgLogHandy
{
    public static class Helper
    {
        public static string GetKfzMatchCode(string KfzKennzeichen)
        {
            var sb = new StringBuilder();
            foreach (var c in KfzKennzeichen.ToUpper())
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(char.ToUpper(c));
            }
            return sb.ToString();
        }

        public static DateTime GetDateTime()
        {
            var d = DateTime.Now;
            return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
        }

        public static string StatusFehlerAnzeige(ApiStatusArten status, string fehlerText = null)
        {
            var grund = string.IsNullOrWhiteSpace(fehlerText) ? "" : " Grund: " + fehlerText + ".";

            return status switch
            {
                ApiStatusArten.DB => "Fehler in  Datenbank!" + grund,
                ApiStatusArten.Intern => $"Interner Fehler!" + grund,
                ApiStatusArten.SI => "Session ID is ungültig." + (string.IsNullOrWhiteSpace(fehlerText) ? "" : $" Fahrzeuge wurde von {fehlerText} übernommen."),
                ApiStatusArten.RE => "Handy Regisitrierungsfehler." + grund,
                ApiStatusArten.WZ => "Konto Fahrer gesperrt. Melden Sie sich bei einem Verantwortlichen." + grund,
                ApiStatusArten.KB => "Sie haben keine Berechtigung für das Fahrzeug." + grund,
                ApiStatusArten.Ok => null,
                _ => "Fehler in Statusart"
            };
        }
    }
}
