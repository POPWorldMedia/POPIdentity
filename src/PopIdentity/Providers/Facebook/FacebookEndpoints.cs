namespace PopIdentity.Providers.Facebook
{
	public static class FacebookEndpoints
	{
		public static string OAuthLoginUrl => "https://www.facebook.com/v3.3/dialog/oauth";
		public static string OAuthAccessTokenBaseUrl => "https://graph.facebook.com/v3.3/oauth/access_token";
		public static string ProfileBaseUrl => "https://graph.facebook.com/me";
	}
}