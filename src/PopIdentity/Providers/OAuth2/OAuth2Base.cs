using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace PopIdentity.Providers.OAuth2
{
	public abstract class OAuth2Base<T> where T: class
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IStateHashingService _stateHashingService;

		protected OAuth2Base(IHttpContextAccessor httpContextAccessor, IStateHashingService stateHashingService)
		{
			_httpContextAccessor = httpContextAccessor;
			_stateHashingService = stateHashingService;
		}

		public abstract T PopulateModel(IEnumerable<Claim> claims);
		public abstract string AccessTokenUrl { get; }

		public async Task<CallbackResult<T>> VerifyCallback(string redirectUri, string clientID, string clientSecret)
		{
			// state check
			var isStateCorrect = _stateHashingService.VerifyHashAgainstCookie();
			if (!isStateCorrect)
				return new CallbackResult<T> { IsSuccessful = false, Message = "State did not match for OAuth2." };

			// get JWT
			var code = _httpContextAccessor.HttpContext.Request.Query["code"];
			var client = new HttpClient();
			var values = new Dictionary<string, string>
			{
				{"code", code},
				{"client_id", clientID},
				{"client_secret", clientSecret},
				{"redirect_uri", redirectUri},
				{"grant_type", "authorization_code"}
			};
			var result = await client.PostAsync(AccessTokenUrl, new FormUrlEncodedContent(values));
			if (!result.IsSuccessStatusCode)
				return new CallbackResult<T> { IsSuccessful = false, Message = $"OAuth2 failed: {result.StatusCode}" };

			// parse results
			var text = await result.Content.ReadAsStringAsync();
			var idToken = JObject.Parse(text).Root.SelectToken("id_token").ToString();
			var handler = new JwtSecurityTokenHandler();
			var token = handler.ReadJwtToken(idToken);
			if (token.Claims == null)
				throw new Exception("OAuth token has no claims");
			var resultModel = PopulateModel(token.Claims);
			return new CallbackResult<T> { IsSuccessful = true, ResultData = resultModel };
		}
	}
}