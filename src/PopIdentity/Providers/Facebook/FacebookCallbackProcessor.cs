﻿using System.Linq;
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
		Task<CallbackResult<FacebookResult>> VerifyCallback(string redirectUri);
		Task<CallbackResult<FacebookResult>> VerifyCallback(string redirectUri, string appID, string appSecret);
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

		public async Task<CallbackResult<FacebookResult>> VerifyCallback(string redirectUri)
		{
			var appID = _popIdentityConfig.FacebookAppID;
			var appSecret = _popIdentityConfig.FacebookAppSecret;
			return await VerifyCallback(redirectUri, appID, appSecret);
		}

		public async Task<CallbackResult<FacebookResult>> VerifyCallback(string redirectUri, string appID, string appSecret)
		{
			// https://developers.facebook.com/docs/facebook-login/manually-build-a-login-flow

			// state check
			var isStateCorrect = _stateHashingService.VerifyHashAgainstCookie();
			if (!isStateCorrect)
				return new CallbackResult<FacebookResult> {IsSuccessful = false, Message = "State did not match for Facebook."};

			// verify OAuth code
			var code = _httpContextAccessor.HttpContext.Request.Query["code"];
			var urlEncodedRedirect = HttpUtility.UrlEncode(redirectUri);
			var client = new HttpClient();
			var result = await client.GetAsync($"{FacebookEndpoints.OAuthAccessTokenBaseUrl}?client_id={appID}&redirect_uri={urlEncodedRedirect}&client_secret={appSecret}&code={code}");
			if (!result.IsSuccessStatusCode)
				return new CallbackResult<FacebookResult> {IsSuccessful = false, Message = $"Facebook OAuth failed: {result.StatusCode}"};

			// get the profile info
			var text = await result.Content.ReadAsStringAsync();
			var idToken = JObject.Parse(text).Root.SelectToken("access_token").ToString();
			var userInfoResponse = await client.GetAsync($"{FacebookEndpoints.ProfileBaseUrl}?access_token={idToken}&fields=id,name,email");
			if (!userInfoResponse.IsSuccessStatusCode)
			{
				var errorText = await userInfoResponse.Content.ReadAsStringAsync();
				var errorMessage = JObject.Parse(errorText).SelectToken("error.message");
				return new CallbackResult<FacebookResult> { IsSuccessful = false, Message = $"Facebook profile retrieval failed: {result.StatusCode}, {errorMessage}" };
			}

			// parse the profile result
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