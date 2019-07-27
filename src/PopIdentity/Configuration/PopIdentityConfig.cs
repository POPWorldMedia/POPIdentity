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
		/// <summary>
		/// Reads the configuration value at PopIdentity:OAuth2Jwt:ClientID
		/// </summary>
		string OAuth2ClientID { get; }
		/// <summary>
		/// Reads the configuration value at PopIdentity:OAuth2Jwt:ClientSecret
		/// </summary>
		string OAuth2ClientSecret { get; }
		/// <summary>
		/// Reads the configuration value at PopIdentity:OAuth2Jwt:LoginUrl
		/// </summary>
		string OAuth2LoginUrl { get; }
		/// <summary>
		/// Reads the configuration value at PopIdentity:OAuth2Jwt:TokenUrl
		/// </summary>
		string OAuth2TokenUrl { get; }
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

        public string OAuth2ClientID => _configuration["PopIdentity:OAuth2Jwt:ClientID"];
        public string OAuth2ClientSecret => _configuration["PopIdentity:OAuth2Jwt:ClientSecret"];
        public string OAuth2LoginUrl => _configuration["PopIdentity:OAuth2Jwt:LoginUrl"];
        public string OAuth2TokenUrl => _configuration["PopIdentity:OAuth2Jwt:TokenUrl"];
	}
}