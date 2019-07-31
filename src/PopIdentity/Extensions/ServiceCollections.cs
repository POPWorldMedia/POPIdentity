using Microsoft.Extensions.DependencyInjection;
using PopIdentity.Configuration;
using PopIdentity.Providers.Facebook;
using PopIdentity.Providers.Google;
using PopIdentity.Providers.Microsoft;
using PopIdentity.Providers.OAuth2;

namespace PopIdentity.Extensions
{
	public static class ServiceCollections
	{
		public static IServiceCollection AddPopIdentity(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddTransient<ILoginLinkFactory, LoginLinkFactory>();
			serviceCollection.AddTransient<IStateHashingService, StateHashingService>();

			serviceCollection.AddTransient<IPopIdentityConfig, PopIdentityConfig>();

			serviceCollection.AddTransient<IFacebookCallbackProcessor, FacebookCallbackProcessor>();

			serviceCollection.AddTransient<IGoogleCallbackProcessor, GoogleCallbackProcessor>();

			serviceCollection.AddTransient<IMicrosoftCallbackProcessor, MicrosoftCallbackProcessor>();

			serviceCollection.AddTransient<IOAuth2JwtCallbackProcessor, OAuth2JwtJwtCallbackProcessor>();

			return serviceCollection;
		}
	}
}