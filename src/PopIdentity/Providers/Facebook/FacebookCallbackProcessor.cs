using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using PopIdentity.Configuration;

namespace PopIdentity.Providers.Facebook
{
	public interface IFacebookCallbackProcessor
	{
		Task<CallbackResult<FacebookResult>> VerifyCallback(HttpRequest request, string redirectUri);
		Task<CallbackResult<FacebookResult>> VerifyCallback(HttpRequest request, string redirectUri, string appID, string appSecret);
	}

	public class FacebookCallbackProcessor : IFacebookCallbackProcessor
	{
		private readonly IPopIdentityConfig _popIdentityConfig;

		public FacebookCallbackProcessor(IPopIdentityConfig popIdentityConfig)
		{
			_popIdentityConfig = popIdentityConfig;
		}

		public async Task<CallbackResult<FacebookResult>> VerifyCallback(HttpRequest request, string redirectUri)
		{
			var appID = _popIdentityConfig.FacebookAppID;
			var appSecret = _popIdentityConfig.FacebookAppSecret;
			return await VerifyCallback(request, redirectUri, appID, appSecret);
		}

		public async Task<CallbackResult<FacebookResult>> VerifyCallback(HttpRequest request, string redirectUri, string appID, string appSecret)
		{
			var code = request.Query["code"];
			var urlEncodedRedirect = HttpUtility.UrlEncode(redirectUri);
			var client = new HttpClient();
			var result = await client.GetAsync($"{FacebookEndpoints.OAuthAccessTokenBaseUrl}?client_id={appID}&redirect_uri={urlEncodedRedirect}&client_secret={appSecret}&code={code}");
			var text = await result.Content.ReadAsStringAsync();
			var idToken = JObject.Parse(text).Root.SelectToken("access_token").ToString();
			var userInfoResponse = await client.GetAsync($"{FacebookEndpoints.ProfileBaseUrl}?access_token={idToken}&fields=id,name,email");
			var userInfoText = await userInfoResponse.Content.ReadAsStringAsync();
			var properties = JObject.Parse(userInfoText).Values().Select(x => new { x.Path, Value = x.Value<string>() }).ToList();
			var facebookResult = new FacebookResult
			{
				ID = properties.FirstOrDefault(x => x.Path == "id")?.Value,
				Email = properties.FirstOrDefault(x => x.Path == "email")?.Value,
				Name = properties.FirstOrDefault(x => x.Path == "name")?.Value
			};
			return new CallbackResult<FacebookResult> {IsSuccessful = true, Message = string.Empty, ResultData = facebookResult};
		}
	}
}