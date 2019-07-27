using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PopIdentity.Configuration;
using PopIdentity.Providers.OAuth2;

namespace PopIdentity.Providers.Google
{
	public interface IGoogleCallbackProcessor
	{
		Task<CallbackResult<GoogleResult>> VerifyCallback(string redirectUri);
		Task<CallbackResult<GoogleResult>> VerifyCallback(string redirectUri, string clientID, string clientSecret);
	}

	public class GoogleCallbackProcessor : OAuth2Base<GoogleResult>, IGoogleCallbackProcessor
	{
		private readonly IPopIdentityConfig _popIdentityConfig;

		public GoogleCallbackProcessor(IPopIdentityConfig popIdentityConfig, IStateHashingService stateHashingService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor, stateHashingService)
		{
			_popIdentityConfig = popIdentityConfig;
		}

		public override string AccessTokenUrl => GoogleEndpoints.OAuthAccessTokenUrl;

		public override GoogleResult PopulateModel(IEnumerable<Claim> claims)
		{
			var list = claims.ToList();
			var resultModel = new GoogleResult
			{
				ID = list.FirstOrDefault(x => x.Type == "sub")?.Value,
				Name = list.FirstOrDefault(x => x.Type == "name")?.Value,
				Email = list.FirstOrDefault(x => x.Type == "email")?.Value,
				FamilyName = list.FirstOrDefault(x => x.Type == "family_name")?.Value,
				GivenName = list.FirstOrDefault(x => x.Type == "given_name")?.Value,
				Picture = list.FirstOrDefault(x => x.Type == "picture")?.Value
			};
			return resultModel;
		}

		public async Task<CallbackResult<GoogleResult>> VerifyCallback(string redirectUri)
		{
			var clientID = _popIdentityConfig.GoogleClientID;
			var clientSecret = _popIdentityConfig.GoogleClientSecret;
			return await VerifyCallback(redirectUri, clientID, clientSecret);
		}
	}
}