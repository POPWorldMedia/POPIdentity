using System.Collections.Generic;
using System.Web;

namespace PopIdentity.Providers.OAuth2
{
	public class OAuth2LoginUrlGenerator
	{
		public string GetUrl(string baseUrl, string clientID, string redirectUrl, string state, IEnumerable<string> scopes)
		{
			var joinedScopes = HttpUtility.UrlEncode(string.Join(" ", scopes));
			return GetUrl(baseUrl, clientID, redirectUrl, state, joinedScopes);
		}
		public string GetUrl(string baseUrl, string clientID, string redirectUrl, string state, string scopes)
		{
			var urlEncodedRedirect = HttpUtility.UrlEncode(redirectUrl);
			var urlEncodedState = HttpUtility.UrlEncode(state);
			var link = $"{baseUrl}?client_id={clientID}&redirect_uri={urlEncodedRedirect}&state={urlEncodedState}&response_type=code&scope={scopes}";
			return link;
		}
	}
}