using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using PopIdentity.Configuration;

namespace PopIdentity.Providers.Facebook
{
	public interface IFacebookCallbackProcessor
	{
		Task<CallbackResult> VerifyCallback(string redirectUri);
		Task<CallbackResult> VerifyCallback(string redirectUri, string appID, string appSecret);
	}

	public class FacebookCallbackProcessor : IFacebookCallbackProcessor
	{
		private readonly IPopIdentityConfig _popIdentityConfig;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IStateHashingService _stateHashingService;

		public FacebookCallbackProcessor(IPopIdentityConfig popIdentityConfig, IHttpContextAccessor httpContextAccessor, IStateHashingService stateHashingService)
		{
			_popIdentityConfig = popIdentityConfig;
			_httpContextAccessor = httpContextAccessor;
			_stateHashingService = stateHashingService;
		}

		public async Task<CallbackResult> VerifyCallback(string redirectUri)
		{
			var appID = _popIdentityConfig.FacebookAppID;
			var appSecret = _popIdentityConfig.FacebookAppSecret;
			return await VerifyCallback(redirectUri, appID, appSecret);
		}

		public async Task<CallbackResult> VerifyCallback(string redirectUri, string appID, string appSecret)
		{
			// state check
			var isStateCorrect = _stateHashingService.VerifyHashAgainstCookie();
			if (!isStateCorrect)
				return new CallbackResult {IsSuccessful = false, Message = "State did not match for Facebook.", ProviderType = ProviderType.Facebook };

			// verify OAuth code
			var code = _httpContextAccessor.HttpContext.Request.Query["code"];
			var urlEncodedRedirect = HttpUtility.UrlEncode(redirectUri);
			string userInfoText = string.Empty;
			using (var client = new HttpClient())
			{
				try
				{
					var result = await client.GetAsync($"{FacebookEndpoints.OAuthAccessTokenBaseUrl}?client_id={appID}&redirect_uri={urlEncodedRedirect}&client_secret={appSecret}&code={code}");
					if (!result.IsSuccessStatusCode)
						return new CallbackResult {IsSuccessful = false, Message = $"Facebook OAuth failed: {result.StatusCode}", ProviderType = ProviderType.Facebook};
					// get the profile info
					var text = await result.Content.ReadAsStringAsync();
					var idToken = JObject.Parse(text).Root.SelectToken("access_token").ToString();
					var userInfoResponse = await client.GetAsync($"{FacebookEndpoints.ProfileBaseUrl}?access_token={idToken}&fields=id,name,email");
					if (!userInfoResponse.IsSuccessStatusCode)
					{
						var errorText = await userInfoResponse.Content.ReadAsStringAsync();
						var errorMessage = JObject.Parse(errorText).SelectToken("error.message");
						return new CallbackResult { IsSuccessful = false, Message = $"Facebook profile retrieval failed: {result.StatusCode}, {errorMessage}", ProviderType = ProviderType.Facebook };
					}
					// parse the profile result
					userInfoText = await userInfoResponse.Content.ReadAsStringAsync();
				}
				catch (HttpRequestException exception)
				{
					return new CallbackResult { IsSuccessful = false, Message = $"Callback for Facebook data failed: {exception.Message}", ProviderType = ProviderType.Facebook };
				}
			}
			var properties = JObject.Parse(userInfoText).Values().Select(x => new { x.Path, Value = x.Value<string>() }).ToList();
			var facebookResult = new ResultData
			{
				ID = properties.FirstOrDefault(x => x.Path == "id")?.Value,
				Email = properties.FirstOrDefault(x => x.Path == "email")?.Value,
				Name = properties.FirstOrDefault(x => x.Path == "name")?.Value
			};
			return new CallbackResult {IsSuccessful = true, Message = string.Empty, ResultData = facebookResult, Claims = new Claim[]{}, ProviderType = ProviderType.Facebook};
		}
	}
}