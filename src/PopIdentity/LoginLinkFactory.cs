using System;
using PopIdentity.Configuration;
using PopIdentity.Providers.Facebook;
using PopIdentity.Providers.Google;
using PopIdentity.Providers.Microsoft;

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
				case ProviderType.Microsoft:
					return GetLink(providerType, redirectUrl, state, _popIdentityConfig.MicrosoftApplicationID);
				default:
					throw new ArgumentException($"No link generator has been defined for {nameof(ProviderType)} \"{providerType}.\"");
			}
		}

		public string GetLink(ProviderType providerType, string redirectUrl, string state, string id)
		{
			switch (providerType)
			{
				case ProviderType.Facebook:
					var facebookLinkGenerator = new FacebookLoginUrlGenerator();
					var facebookLink = facebookLinkGenerator.GetUrl(FacebookEndpoints.OAuthLoginUrl, id, redirectUrl, state);
					return facebookLink;
				case ProviderType.Google:
					var googleLinkGenerator = new GoogleLoginUrlGenerator();
					var googleLink = googleLinkGenerator.GetUrl(GoogleEndpoints.OAuthLoginUrl, id, redirectUrl, state);
					return googleLink;
				case ProviderType.Microsoft:
					var msftGenerator = new MicrosoftLoginUrlGenerator();
					var msftLink = msftGenerator.GetUrl(MicrosoftEndpoints.OAuthLoginUrl, id, redirectUrl, state);
					return msftLink;
				default:
					throw new ArgumentException($"No link generator has been defined for {nameof(ProviderType)} \"{providerType}.\"");
			}
		}
	}
}