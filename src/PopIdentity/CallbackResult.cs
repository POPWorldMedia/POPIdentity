using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PopIdentity
{
	public class CallbackResult
	{
		public ResultData ResultData { get; set; }
		public bool IsSuccessful { get; set; }
		public string Message { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
		public ProviderType ProviderType { get; set; }
		public JwtSecurityToken Token { get; set; }
		public string RefreshToken { get; set; }
		public string AccessToken { get; set; }
	}
}