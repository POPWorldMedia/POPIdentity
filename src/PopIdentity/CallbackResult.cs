using System.Collections.Generic;
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
	}
}