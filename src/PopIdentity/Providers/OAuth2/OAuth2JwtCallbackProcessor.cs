using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PopIdentity.Configuration;

namespace PopIdentity.Providers.OAuth2
{
	public interface IOAuth2JwtCallbackProcessor
	{
		Task<CallbackResult> VerifyCallback(string redirectUri, string accessTokenUrl);
		Task<CallbackResult> VerifyCallback(string redirectUri, string accessTokenUrl, string applicationID, string clientSecret);
	}

	public class OAuth2JwtJwtCallbackProcessor : OAuth2BaseProcessor, IOAuth2JwtCallbackProcessor
	{
		private readonly IPopIdentityConfig _popIdentityConfig;

		public OAuth2JwtJwtCallbackProcessor(IHttpContextAccessor httpContextAccessor, IStateHashingService stateHashingService, IPopIdentityConfig popIdentityConfig) : base(httpContextAccessor, stateHashingService)
		{
			_popIdentityConfig = popIdentityConfig;
		}

		private string _accessTokenUrl;

		public override string AccessTokenUrl => _accessTokenUrl;

		public async Task<CallbackResult> VerifyCallback(string redirectUri, string accessTokenUrl)
		{
			_accessTokenUrl = accessTokenUrl;
			var applicationID = _popIdentityConfig.OAuth2ClientID;
			var clientSecret = _popIdentityConfig.OAuth2ClientSecret;
			return await VerifyCallback(redirectUri, applicationID, clientSecret);
		}

		public async Task<CallbackResult> VerifyCallback(string redirectUri, string accessTokenUrl, string applicationID, string clientSecret)
		{
			_accessTokenUrl = accessTokenUrl;
			return await VerifyCallback(redirectUri, applicationID, clientSecret);
		}
	}
}