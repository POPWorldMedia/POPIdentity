using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PopIdentity.Providers.OAuth2
{
	public abstract class OAuth2BaseProcessor
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IStateHashingService _stateHashingService;

		protected OAuth2BaseProcessor(IHttpContextAccessor httpContextAccessor, IStateHashingService stateHashingService)
		{
			_httpContextAccessor = httpContextAccessor;
			_stateHashingService = stateHashingService;
		}

		public abstract string AccessTokenUrl { get; }
		public abstract ProviderType ProviderType { get; }

		public async Task<CallbackResult> VerifyCallback(string redirectUri, string clientID, string clientSecret)
		{
			// state check
			var isStateCorrect = _stateHashingService.VerifyHashAgainstCookie();
			if (!isStateCorrect)
				return new CallbackResult { IsSuccessful = false, Message = "State did not match for OAuth2.", ProviderType = ProviderType.OAuth2 };

			// get JWT
			var code = _httpContextAccessor.HttpContext.Request.Query["code"];
			var values = new Dictionary<string, string>
			{
				{"code", code},
				{"client_id", clientID},
				{"client_secret", clientSecret},
				{"redirect_uri", redirectUri},
				{"grant_type", "authorization_code"}
			};
			HttpResponseMessage result;
			using (var client = new HttpClient())
			{
				try
				{
					result = await client.PostAsync(AccessTokenUrl, new FormUrlEncodedContent(values));
				}
				catch (HttpRequestException exception)
				{
					return new CallbackResult { IsSuccessful = false, Message = $"Callback for token failed: {exception.Message}", ProviderType = ProviderType.OAuth2 };
				}
			}
			if (!result.IsSuccessStatusCode)
				return new CallbackResult { IsSuccessful = false, Message = $"OAuth2 failed: {result.StatusCode}", ProviderType = ProviderType.OAuth2 };

			// parse results
			var text = await result.Content.ReadAsStringAsync();
			JsonDocument.Parse(text).RootElement.TryGetProperty("id_token", out var idToken);
			var handler = new JwtSecurityTokenHandler();
			var token = handler.ReadJwtToken(idToken.GetString());
			if (token.Claims == null)
				throw new Exception("OAuth token has no claims");
            var resultModel = new ResultData
            {
                ID = token.Claims.FirstOrDefault(x => x.Type == "sub")?.Value,
                Name = token.Claims.FirstOrDefault(x => x.Type == "name")?.Value,
                Email = token.Claims.FirstOrDefault(x => x.Type == "email")?.Value
            };
			return new CallbackResult { IsSuccessful = true, ResultData = resultModel, Claims = token.Claims, ProviderType = ProviderType };
		}
	}
}