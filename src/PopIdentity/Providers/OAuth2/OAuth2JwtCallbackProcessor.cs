using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PopIdentity.Configuration;

namespace PopIdentity.Providers.OAuth2
{
	public interface IOAuth2JwtCallbackProcessor<T> where T : class
	{
		Task<CallbackResult<T>> VerifyCallback(string redirectUri, Func<IEnumerable<Claim>, T> claimMapper, string accessTokenUrl);
	}

	public class IoAuth2JwtJwtCallbackProcessor<T> : OAuth2Base<T>, IOAuth2JwtCallbackProcessor<T> where T : class
	{
		private readonly IPopIdentityConfig _popIdentityConfig;

		public IoAuth2JwtJwtCallbackProcessor(IHttpContextAccessor httpContextAccessor, IStateHashingService stateHashingService, IPopIdentityConfig popIdentityConfig) : base(httpContextAccessor, stateHashingService)
		{
			_popIdentityConfig = popIdentityConfig;
		}

		private Func<IEnumerable<Claim>, T> _claimMapper;
		private string _accessTokenUrl;

		public override string AccessTokenUrl => _accessTokenUrl;

		public async Task<CallbackResult<T>> VerifyCallback(string redirectUri, Func<IEnumerable<Claim>, T> claimMapper, string accessTokenUrl)
		{
			_accessTokenUrl = accessTokenUrl;
			_claimMapper = claimMapper;
			var applicationID = _popIdentityConfig.OAuth2ClientID;
			var clientSecret = _popIdentityConfig.OAuth2ClientSecret;
			return await VerifyCallback(redirectUri, applicationID, clientSecret);
		}

		public override T PopulateModel(IEnumerable<Claim> claims)
		{
			var list = claims.ToList();
			var resultModel = _claimMapper(list);
			return resultModel;
		}
	}
}