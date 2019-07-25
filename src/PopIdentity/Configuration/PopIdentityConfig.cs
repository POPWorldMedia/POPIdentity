using Microsoft.Extensions.Configuration;

namespace PopIdentity.Configuration
{
	public interface IPopIdentityConfig
	{
		/// <summary>
		/// Reads the configuration value at PopIdentity:Facebook:AppID.
		/// </summary>
		string FacebookAppID { get; }
		/// <summary>
		/// Reads the configuration value at PopIdentity:Facebook:AppSecret.
		/// </summary>
		string FacebookAppSecret { get; }
		/// <summary>
		/// Reads the configuration value at PopIdentity:Google:ClientID
		/// </summary>
		string GoogleClientID { get; }
		/// <summary>
		/// Reads the configuration value at PopIdentity:Google:ClientSecret
		/// </summary>
		string GoogleClientSecret { get; }
		/// <summary>
		/// Reads the configuration value at PopIdentity:Microsoft:ApplicationID
		/// </summary>
		string MicrosoftApplicationID { get; }
		/// <summary>
		/// Reads the configuration value at PopIdentity:Microsoft:ClientSecret
		/// </summary>
		string MicrosoftClientSecret { get; }
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

        public string MicrosoftApplicationID => _configuration["PopIdentity:Microsoft:ApplicationID"];
        public string MicrosoftClientSecret => _configuration["PopIdentity:Microsoft:ClientSecret"];
	}
}