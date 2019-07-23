using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using PopIdentity.Configuration;

namespace PopIdentity.Providers.Google
{
	public interface IGoogleCallbackProcessor
	{
		Task<CallbackResult<GoogleResult>> VerifyCallback(string redirectUri);
		Task<CallbackResult<GoogleResult>> VerifyCallback(string redirectUri, string clientID, string clientSecret);
	}

	public class GoogleCallbackProcessor : IGoogleCallbackProcessor
	{
		private readonly IPopIdentityConfig _popIdentityConfig;
		private readonly IStateHashingService _stateHashingService;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GoogleCallbackProcessor(IPopIdentityConfig popIdentityConfig, IStateHashingService stateHashingService, IHttpContextAccessor httpContextAccessor)
		{
			_popIdentityConfig = popIdentityConfig;
			_stateHashingService = stateHashingService;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<CallbackResult<GoogleResult>> VerifyCallback(string redirectUri)
		{
			var clientID = _popIdentityConfig.GoogleClientID;
			var clientSecret = _popIdentityConfig.GoogleClientSecret;
			return await VerifyCallback(redirectUri, clientID, clientSecret);
		}

		public async Task<CallbackResult<GoogleResult>> VerifyCallback(string redirectUri, string clientID, string clientSecret)
		{
			// https://developers.google.com/identity/protocols/OpenIDConnect#server-flow

			// state check
			var isStateCorrect = _stateHashingService.VerifyHashAgainstCookie();
			if (!isStateCorrect)
				return new CallbackResult<GoogleResult> { IsSuccessful = false, Message = "State did not match for Google." };

			// get JWT
			var code = _httpContextAccessor.HttpContext.Request.Query["code"];
			var client = new HttpClient();
			var values = new Dictionary<string, string>
			{
				{"code", code},
				{"client_id", clientID},
				{"client_secret", clientSecret},
				{"redirect_uri", "https://localhost:44353/home/callbackgoogle"},
				{"grant_type", "authorization_code"}
			};
			var result = await client.PostAsync(GoogleEndpoints.OAuthAccessTokenUrl, new FormUrlEncodedContent(values));
			if (!result.IsSuccessStatusCode)
				return new CallbackResult<GoogleResult> { IsSuccessful = false, Message = $"Google OAuth failed: {result.StatusCode}" };

			// parse results
			var text = await result.Content.ReadAsStringAsync();
			var idToken = JObject.Parse(text).Root.SelectToken("id_token").ToString();
			var handler = new JwtSecurityTokenHandler();
			var token = handler.ReadJwtToken(idToken);
			if (token.Claims == null)
				throw new Exception("Google token has no claims");
			var resultModel = new GoogleResult
			{
				ID = token.Claims.FirstOrDefault(x => x.Type == "sub")?.Value,
				Name = token.Claims.FirstOrDefault(x => x.Type == "name")?.Value,
				Email = token.Claims.FirstOrDefault(x => x.Type == "email")?.Value,
				FamilyName = token.Claims.FirstOrDefault(x => x.Type == "family_name")?.Value,
				GivenName = token.Claims.FirstOrDefault(x => x.Type == "given_name")?.Value,
				Picture = token.Claims.FirstOrDefault(x => x.Type == "picture")?.Value
			};
			return new CallbackResult<GoogleResult> {IsSuccessful = true, ResultData = resultModel};
		}
	}
}