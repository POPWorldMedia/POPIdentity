using Microsoft.Extensions.Configuration;

namespace PopIdentity.Configuration
{
	public interface IPopIdentityConfig
	{
		string FacebookAppID { get; }
		string FacebookAppSecret { get; }
		string GoogleClientID { get; }
		string GoogleClientSecret { get; }
	}

	public class PopIdentityConfig : IPopIdentityConfig
	{
		private readonly IConfiguration _configuration;

		public PopIdentityConfig(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string FacebookAppID => _configuration["PopIdentity:Facebook:AppID"];
		public string FacebookAppSecret => _configuration["PopIdentity:Facebook:AppSecret"];

		public string GoogleClientID => _configuration["PopIdentity:Google:ClientID"];
		public string GoogleClientSecret => _configuration["PopIdentity:Google:ClientSecret"];
	}
}