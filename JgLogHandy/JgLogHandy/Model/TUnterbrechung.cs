using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

namespace JgLogHandy
{
    public class TUnterBrechung : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly Timer Uhr = new Timer(1000);
        private readonly DateTime _ZeitStart = DateTime.Now;

        public string AnzeigeZeit => (DateTime.Now - _ZeitStart).ToString(@"dd\ hh\:mm\:ss");
        public int Zeit => (int)Math.Ceiling((DateTime.Now - _ZeitStart).TotalMinutes);

        public TUnterBrechung()
        {
            Uhr.Elapsed += (source, e) => SetProperty(nameof(AnzeigeZeit));
            Uhr.Start();
        }
    }
}
