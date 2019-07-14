using System;
using PopIdentity.Configuration;
using PopIdentity.Providers.Facebook;

namespace PopIdentity
{
	public interface ILoginLinkFactory
	{
		string GetLink(ProviderType providerType, string redirectUrl, string state);
		string GetLink(ProviderType providerType, string redirectUrl, string state, string id);
	}

	public class LoginLinkFactory : ILoginLinkFactory
	{
		private readonly IPopIdentityConfig _popIdentityConfig;

		public LoginLinkFactory(IPopIdentityConfig popIdentityConfig)
		{
			_popIdentityConfig = popIdentityConfig;
		}

		public string GetLink(ProviderType providerType, string redirectUrl, string state)
		{
			switch (providerType)
			{
				case ProviderType.Facebook:
					return GetLink(providerType, redirectUrl, state, _popIdentityConfig.FacebookAppID);
				case ProviderType.Google:
					return GetLink(providerType, redirectUrl, state, _popIdentityConfig.GoogleClientID);
				default:
					throw new ArgumentException($"No link generator has been defined for {nameof(ProviderType)} \"{providerType}.\"");
			}
		}

		public string GetLink(ProviderType providerType, string redirectUrl, string state, string id)
		{
			switch (providerType)
			{
				case ProviderType.Facebook:
					var generator = new FacebookLinkGenerator();
					var link = generator.GetLink(FacebookEndpoints.OAuthLoginLink, id, redirectUrl, state);
					return link;
				case ProviderType.Google:
					throw new NotImplementedException();
				default:
					throw new ArgumentException($"No link generator has been defined for {nameof(ProviderType)} \"{providerType}.\"");
			}
		}
	}
}