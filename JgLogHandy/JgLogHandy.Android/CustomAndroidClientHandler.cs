using Android.Net;
using Javax.Net.Ssl;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Android.Net;

/// <summary>
///  Notwendig für die Übertragung unverschlüsselter http Anforderung für Testzwecke, Ausschalten über Resources/xml/network_security_config.xml
/// </summary>

namespace JgLogHandy.Droid
{
    public class CustomAndroidClientHandler : AndroidClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Version = new System.Version(2, 0);
            return await base.SendAsync(request, cancellationToken);
        }

        protected override SSLSocketFactory ConfigureCustomSSLSocketFactory(HttpsURLConnection connection)
        {
            return SSLCertificateSocketFactory.GetInsecure(0, null);
        }

        protected override IHostnameVerifier GetSSLHostnameVerifier(HttpsURLConnection connection)
        {
            return new BypassHostnameVerifier();
        }
    }

    internal class BypassHostnameVerifier : Java.Lang.Object, IHostnameVerifier
    {
        public bool Verify(string hostname, ISSLSession session)
        {
            return true;
        }
    }
}