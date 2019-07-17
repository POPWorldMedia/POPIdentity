using Microsoft.Extensions.DependencyInjection;
using PopIdentity.Configuration;
using PopIdentity.Providers.Facebook;

namespace PopIdentity.Extensions
{
	public static class ServiceCollections
	{
		public static IServiceCollection AddPopIdentity(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddTransient<ILoginLinkFactory, LoginLinkFactory>();

			serviceCollection.AddTransient<IPopIdentityConfig, PopIdentityConfig>();

			serviceCollection.AddTransient<IFacebookCallbackProcessor, FacebookCallbackProcessor>();

			return serviceCollection;
		}
	}
}