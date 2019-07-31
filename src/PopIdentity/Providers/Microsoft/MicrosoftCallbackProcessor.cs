using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PopIdentity.Configuration;
using PopIdentity.Providers.OAuth2;

namespace PopIdentity.Providers.Microsoft
{
	public interface IMicrosoftCallbackProcessor
	{
		Task<CallbackResult> VerifyCallback(string redirectUri);
		Task<CallbackResult> VerifyCallback(string redirectUri, string applicationID, string clientSecret);
	}

	public class MicrosoftCallbackProcessor : OAuth2Base, IMicrosoftCallbackProcessor
	{
		private readonly IPopIdentityConfig _popIdentityConfig;

		public MicrosoftCallbackProcessor(IStateHashingService stateHashingService, IHttpContextAccessor httpContextAccessor, IPopIdentityConfig popIdentityConfig) : base(httpContextAccessor, stateHashingService)
		{
			_popIdentityConfig = popIdentityConfig;
		}

		public override string AccessTokenUrl => MicrosoftEndpoints.OAuthAccessTokenUrl;

		public async Task<CallbackResult> VerifyCallback(string redirectUri)
		{
			var applicationID = _popIdentityConfig.MicrosoftApplicationID;
			var clientSecret = _popIdentityConfig.MicrosoftClientSecret;
			return await VerifyCallback(redirectUri, applicationID, clientSecret);
		}
	}
}