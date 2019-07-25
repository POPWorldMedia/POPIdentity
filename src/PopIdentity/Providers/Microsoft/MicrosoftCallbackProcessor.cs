using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using PopIdentity.Configuration;

namespace PopIdentity.Providers.Microsoft
{
	public interface IMicrosoftCallbackProcessor
	{
		Task<CallbackResult<MicrosoftResult>> VerifyCallback(string redirectUri);
		Task<CallbackResult<MicrosoftResult>> VerifyCallback(string redirectUri, string applicationID, string clientSecret);
	}

	public class MicrosoftCallbackProcessor : IMicrosoftCallbackProcessor
	{
		private readonly IStateHashingService _stateHashingService;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IPopIdentityConfig _popIdentityConfig;

		public MicrosoftCallbackProcessor(IStateHashingService stateHashingService, IHttpContextAccessor httpContextAccessor, IPopIdentityConfig popIdentityConfig)
		{
			_stateHashingService = stateHashingService;
			_httpContextAccessor = httpContextAccessor;
			_popIdentityConfig = popIdentityConfig;
		}

		public async Task<CallbackResult<MicrosoftResult>> VerifyCallback(string redirectUri)
		{
			var applicationID = _popIdentityConfig.MicrosoftApplicationID;
			var clientSecret = _popIdentityConfig.MicrosoftClientSecret;
			return await VerifyCallback(redirectUri, applicationID, clientSecret);
		}

		public async Task<CallbackResult<MicrosoftResult>> VerifyCallback(string redirectUri, string applicationID, string clientSecret)
		{
			// state check
			var isStateCorrect = _stateHashingService.VerifyHashAgainstCookie();
			if (!isStateCorrect)
				return new CallbackResult<MicrosoftResult> { IsSuccessful = false, Message = "State did not match for Microsoft." };

			// get JWT
			var code = _httpContextAccessor.HttpContext.Request.Query["code"];
			var client = new HttpClient();
			var values = new Dictionary<string, string>
			{
				{"code", code},
				{"client_id", applicationID},
				{"client_secret", clientSecret},
				{"redirect_uri", redirectUri},
				{"grant_type", "authorization_code"}
			};
			var result = await client.PostAsync(MicrosoftEndpoints.OAuthAccessTokenUrl, new FormUrlEncodedContent(values));
			if (!result.IsSuccessStatusCode)
				return new CallbackResult<MicrosoftResult> { IsSuccessful = false, Message = $"Microsoft OAuth failed: {result.StatusCode}" };

			// parse results
			var text = await result.Content.ReadAsStringAsync();
			var idToken = JObject.Parse(text).Root.SelectToken("id_token").ToString();
			var handler = new JwtSecurityTokenHandler();
			var token = handler.ReadJwtToken(idToken);
			if (token.Claims == null)
				throw new Exception("Microsoft token has no claims");
			var resultModel = new MicrosoftResult
			{
				ID = token.Claims.FirstOrDefault(x => x.Type == "sub")?.Value,
				Name = token.Claims.FirstOrDefault(x => x.Type == "name")?.Value,
				Email = token.Claims.FirstOrDefault(x => x.Type == "email")?.Value
			};
			return new CallbackResult<MicrosoftResult> { IsSuccessful = true, ResultData = resultModel };
		}
	}
}