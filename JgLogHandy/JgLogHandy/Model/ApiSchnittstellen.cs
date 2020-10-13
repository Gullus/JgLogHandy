using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace JgLogHandy
{

    public enum ApiStatusArten : byte
    {
        Ok = 0,         // Ok      
        RE = 1,         // Handy Registrierungsfehler
        DB = 2,         // Fehler durch Datenbank
        SI = 3,         // Session Id Falsch
        WZ = 4,         // Webzugriff gesperrt
        KB = 5,         // Keine Berechtigung für das Fahrzeug,
        LO = 6,         // Fahrer wurde gelöscht

        Intern = 10,    // Interner Fehler, nur für Handy
        SendFehler = 11 // Fehler aus Senden
    }

    #region Interface Helper

    public interface IApiStatus
    {
        /// <summary>
        /// Fehlerart
        /// </summary>
        ApiStatusArten ST { get; set; }
    }

    public interface IApiGps
    {
        /// <summary>
        /// Baustelle Länge
        /// </summary>
        double? BL { get; set; }

        /// <summary>
        /// Baustelle Breite
        /// </summary>
        double? BB { get; set; }
    }

    public interface IApiLieferung : IApiGps
    {
        /// <summary>
        /// Id Lieferung
        /// </summary>
        Guid IL { get; set; }

        /// <summary>
        /// Nummer der Lieferung bestehen aus Baustellennummer-NummerLieferung
        /// </summary>
        string NR { get; set; }

        /// <summary>
        /// Baustellenbezeichnung
        /// </summary>
        string Bst { get; set; }

        /// <summary>
        /// Baustellenort
        /// </summary>
        string Ort { get; set; }

        /// <summary>
        /// Baustelle Strasse
        /// </summary>
        string Str { get; set; }

        /// <summary>
        /// Zeit Bestellung
        /// </summary>
        DateTime ZB { get; set; }

        /// <summary>
        /// Zeit geplant
        /// </summary>
        DateTime ZP { get; set; }

        /// <summary>
        /// Abweichung Minuten
        /// </summary>
        int ZA { get; set; }

        /// <summary>
        /// Menge
        /// </summary>
        float ME { get; set; }

        /// <summary>
        /// Artikel
        /// </summary>
        string AR { get; set; }

        /// <summary>
        /// Einheit
        /// </summary>
        string EH { get; set; }

        /// <summary>
        /// Kundenname
        /// </summary>
        string KU { get; set; }

        /// <summary>
        /// Id des Lieferwerkes
        /// </summary>
        Guid IW { get; set; }

        /// <summary>
        /// Lieferart
        /// </summary>
        LieferArten LA { get; set; }

        /// <summary>
        /// Status aktuelle Lieferung
        /// </summary>
        StatusLieferung ST { get; set; }

        /// <summary>
        /// Modifing des Status, ob Status Lieferung aktualisiert werden muss
        /// </summary>
        public int NS { get; set; }
    }

    public interface IApiWerk : IApiGps
    {
        /// <summary>
        /// Id des Lieferwerkes
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        ///  Werkname
        /// </summary>
        string WN { get; set; }
    }

    public interface IApiAction : IApiGps
    {
        /// <summary>
        /// Kfz Action
        /// </summary>
        KfzActions AC { get; set; }

        /// <summary>
        /// Zeitstempel Auslösung Action
        /// </summary>
        DateTime AZ { get; set; }

        /// <summary>
        /// Id der Lieferung
        /// </summary>
        Guid? IL { get; set; }

        /// <summary>
        /// Nummer Statusänderung
        /// </summary>
        int? NS { get; set; }
    }

    #endregion

    public interface IAntwortRegistrierung : IApiStatus
    {
        /// <summary>
        /// Kennwort Registrierung aus User Webkennwort
        /// </summary>
        string RK { get; set; }

        /// <summary>
        /// Liste Fahrzeuge getrennt durch Semmikolon
        /// </summary>
        string LF { get; set; }

        /// <summary>
        /// Fahrer Name
        /// </summary>
        string FN { get; set; }

        /// <summary>
        /// Infotext
        /// </summary>
        string IT { get; set; }
    }

    public interface IAntwortAnmeldung : IApiStatus
    {
        /// <summary>
        /// Id der Session. Wird bei der Anmeldung im Server erstellt
        /// </summary>
        string SI { get; set; }

        /// <summary>
        /// Guid der aktuellen Lieferung
        /// </summary>
        Guid? AL { get; set; }

        /// <summary>
        /// Liste aller Werke
        /// </summary>
        List<TApiWerk> LWerk { get; set; }

        /// <summary>
        /// Liste der Lieferungen
        /// </summary>
        List<TApiLieferung> LLief { get; set; }

        /// <summary>
        /// größte Modifikationsnummer der Lieferungen
        /// </summary>
        int MF { get; set; }

        /// <summary>
        /// Infotext
        /// </summary>
        string IT { get; set; }

        /// <summary>
        /// geforderte Versionsnummer der App
        /// </summary>
        string VA { get; set; }
    }

    public interface IAntwortLieferungen : IApiStatus
    {
        /// <summary>
        /// Liste der Lieferungen
        /// </summary>
        List<TApiLieferung> LLief { get; set; }

        /// <summary>
        /// größte Modifikationsnummer der Lieferungen
        /// </summary>
        int MF { get; set; }
    }

    public interface IAntwortZusatzInfo : IApiStatus
    {
        /// <summary>
        /// Abweichende Minuten von Planung
        /// </summary>
        int ZM { get; set; }

        /// <summary>
        /// Lieferscheinnummer
        /// </summary>
        string LfsN { get; set; }

        /// <summary>
        /// registrierte Menge von Waage
        /// </summary>
        float? LfsM { get; set; }

        /// <summary>
        /// Partner Name
        /// </summary>
        string PN { get; set; }

        /// <summary>
        /// Partner Handy
        /// </summary>
        string PH { get; set; }

        /// <summary>
        /// Partner Telefon
        /// </summary>
        string PT { get; set; }
    }

    public interface ISetZusatzInfo
    {
        /// <summary>
        /// Id der Session. Wird bei der Anmeldung im Server erstellt
        /// </summary>
        string SI { get; set; }

        /// <summary>
        /// Id Lieferung
        /// </summary>
        Guid IL { get; set; }

        /// <summary>
        /// Abweichende Minuten von Planung
        /// </summary>
        int ZM { get; set; }

        /// <summary>
        /// Lieferscheinnummer
        /// </summary>
        string LfsN { get; set; }

        /// <summary>
        /// registrierte Menge von Waage
        /// </summary>
        float? LfsM { get; set; }
    }

    public interface ISetAction
    {
        /// <summary>
        /// Kfz Kennzeichen
        /// </summary>
        string KK { get; set; }

        /// <summary>
        /// Aktueller KfzAction
        /// </summary>
        KfzActions KA { get; set; }

        /// <summary>
        ///  Liste der Actions
        /// </summary>
        List<TApiAction> LA { get; set; }

        /// <summary>
        /// Aktueller Lieferschein
        /// </summary>
        Guid? AL { get; set; }
    }

    public interface IAntwortInfoString : IApiStatus
    {
        /// <summary>
        /// Text der Information
        /// </summary>
        string INF { get; set; }
    }
}
