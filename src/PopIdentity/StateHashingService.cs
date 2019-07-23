using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace PopIdentity
{
	public interface IStateHashingService
	{
		string SetCookieAndReturnHash();
		bool VerifyHashAgainstCookie(string hash);
	}

	public class StateHashingService : IStateHashingService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private const string CookieName = "pi.state.hash";

		public StateHashingService(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public string SetCookieAndReturnHash()
		{
			var cookieValue = Guid.NewGuid().ToString();
			_httpContextAccessor.HttpContext.Response.Cookies.Append(CookieName, cookieValue);
			var hash = GetHash(cookieValue);
			return hash;
		}

		public bool VerifyHashAgainstCookie(string hash)
		{
			var cookieValue = _httpContextAccessor.HttpContext.Request.Cookies[CookieName];
			if (string.IsNullOrEmpty(cookieValue))
				return false;
			var hashedCookieValue = GetHash(cookieValue);
			_httpContextAccessor.HttpContext.Response.Cookies.Delete(CookieName);
			return hash == hashedCookieValue;
		}

		private string GetHash(string text)
		{
			using (var sha256 = new SHA256Managed())
			{
				var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
				var builder = new StringBuilder();
				foreach (var t in bytes)
					builder.Append(t.ToString("x2"));
				var result = builder.ToString();
				return result;
			}
		}
	}
}