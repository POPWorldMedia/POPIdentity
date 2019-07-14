using System.Web;

namespace PopIdentity.Providers.Facebook
{
	public class FacebookLinkGenerator
	{
		public string GetLink(string baseUrl, string clientID, string redirectUrl, string state)
		{
			var urlEncodedRedirect = HttpUtility.UrlEncode(redirectUrl);
			var urlEncodedState = HttpUtility.UrlEncode(state);
			var link = $"{baseUrl}?client_id={clientID}&redirect_uri={urlEncodedRedirect}&state={urlEncodedState}&response_type=code&scope=email";
			return link;
		}
	}
}