using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PopIdentity.Configuration;
using PopIdentity.Providers.OAuth2;

namespace PopIdentity.Providers.Microsoft
{
	public interface IMicrosoftCallbackProcessor
	{
		Task<CallbackResult<MicrosoftResult>> VerifyCallback(string redirectUri);
		Task<CallbackResult<MicrosoftResult>> VerifyCallback(string redirectUri, string applicationID, string clientSecret);
	}

	public class MicrosoftCallbackProcessor : OAuth2Base<MicrosoftResult>, IMicrosoftCallbackProcessor
	{
		private readonly IPopIdentityConfig _popIdentityConfig;

		public MicrosoftCallbackProcessor(IStateHashingService stateHashingService, IHttpContextAccessor httpContextAccessor, IPopIdentityConfig popIdentityConfig) : base(httpContextAccessor, stateHashingService)
		{
			_popIdentityConfig = popIdentityConfig;
		}

		public override string AccessTokenUrl => MicrosoftEndpoints.OAuthAccessTokenUrl;

		public async Task<CallbackResult<MicrosoftResult>> VerifyCallback(string redirectUri)
		{
			var applicationID = _popIdentityConfig.MicrosoftApplicationID;
			var clientSecret = _popIdentityConfig.MicrosoftClientSecret;
			return await VerifyCallback(redirectUri, applicationID, clientSecret);
		}

		public override MicrosoftResult PopulateModel(IEnumerable<Claim> claims)
		{
			var list = claims.ToList();
			var resultModel = new MicrosoftResult
			{
				ID = list.FirstOrDefault(x => x.Type == "sub")?.Value,
				Name = list.FirstOrDefault(x => x.Type == "name")?.Value,
				Email = list.FirstOrDefault(x => x.Type == "email")?.Value
			};
			return resultModel;
		}
	}
}