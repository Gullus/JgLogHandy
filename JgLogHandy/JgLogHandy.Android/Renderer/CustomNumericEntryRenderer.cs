using Android.Content;
using JgLogHandy;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomNumericEntry), typeof(JgLogHandy.Droid.CustomNumericEntryRenderer))]
namespace JgLogHandy.Droid
{
    public class CustomNumericEntryRenderer : EntryRenderer
    {
        public CustomNumericEntryRenderer(Context context) 
            : base(context)
        { }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (Control == null || Element == null) 
                return;

            Control.KeyListener = Android.Text.Method.DigitsKeyListener.GetInstance(string.Format("1234567890{0}", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
            Control.InputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal;
        }
    }
}