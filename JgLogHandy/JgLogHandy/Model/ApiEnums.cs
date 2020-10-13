namespace JgLogHandy
{
    public enum StatusLieferung : byte
    {
        Offen = 0,
        Angenommen,
        Beladung,
        Anfahrt,
        Ankunft,
        Entladung,
        Fertig,
        Storniert,
    }

    public enum LieferArten : byte
    {
        Lieferung = 0,
        Anlieferung = 1,
        Verleih = 2
    }
    public enum KfzActions : byte
    {
        Anmeldung = 0,
        Abmeldung,
        Pause,
        Reparatur,
        Angenommen,
        Annullieren,
        Beladung,
        Anfahrt,
        Ankunft,
        Entladung,
        Fertig,
        Fehler = 255
    }

    public enum DialogArten
    {
        Info,
        Warnung,
        Fehler
    }
}
