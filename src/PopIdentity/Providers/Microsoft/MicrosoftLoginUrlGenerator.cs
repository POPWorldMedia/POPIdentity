using System.Web;

namespace PopIdentity.Providers.Microsoft
{
	public class MicrosoftLoginUrlGenerator
	{
		public string GetUrl(string baseUrl, string clientID, string redirectUrl, string state)
		{
			var urlEncodedRedirect = HttpUtility.UrlEncode(redirectUrl);
			var urlEncodedState = HttpUtility.UrlEncode(state);
			var link = $"{baseUrl}?client_id={clientID}&redirect_uri={urlEncodedRedirect}&state={urlEncodedState}&response_type=code&scope=openid+email";
			return link;
		}
	}
}