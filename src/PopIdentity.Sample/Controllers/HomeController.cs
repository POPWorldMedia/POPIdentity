using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PopIdentity.Configuration;
using PopIdentity.Providers.Facebook;
using PopIdentity.Sample.Models;

namespace PopIdentity.Sample.Controllers
{
    public class HomeController : Controller
    {
	    private readonly IPopIdentityConfig _popIdentityConfig;
	    private readonly ILoginLinkFactory _loginLinkFactory;
	    private readonly IFacebookCallbackProcessor _facebookCallbackProcessor;

	    public HomeController(IPopIdentityConfig popIdentityConfig, ILoginLinkFactory loginLinkFactory, IFacebookCallbackProcessor facebookCallbackProcessor)
	    {
		    _popIdentityConfig = popIdentityConfig;
		    _loginLinkFactory = loginLinkFactory;
		    _facebookCallbackProcessor = facebookCallbackProcessor;
	    }

	    public IActionResult Index()
	    {
		    ViewBag.GoogleLink = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={_popIdentityConfig.GoogleClientID}&redirect_uri=https%3A%2F%2Flocalhost:44353%2Fhome%2Fcallback&scope=https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.email+https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.profile+openid+email+profile&state=1234&response_type=code";

			// This URL has to be specified in the Facebook developer console under "Valid OAuth Redirect URIs."
			var facebookRedirect = "https://localhost:44353/home/callbackfb";
		    ViewBag.FacebookLink = _loginLinkFactory.GetLink(ProviderType.Facebook, facebookRedirect, "1234");
            return View();
        }

        public async Task<IActionResult> CallbackFB()
        {
			// https://developers.facebook.com/docs/facebook-login/manually-build-a-login-flow
			// check for state match
			var result = await _facebookCallbackProcessor.VerifyCallback(Request, "https://localhost:44353/home/callbackfb");
			if (!result.IsSuccessful)
				return Content(result.Message);
			var list = $"id: {result.ResultData.ID}\r\nname: {result.ResultData.Name}\r\nemail: {result.ResultData.Email}";
			return Content(list);
        }

		public async Task<IActionResult> Callback()
        {
			// https://developers.google.com/identity/protocols/OpenIDConnect#server-flow
			// check for state match
			// get code
			var code = Request.Query["code"];
            var client = new HttpClient();
            var values = new Dictionary<string, string>()
            {
                {"code", code},
                {"client_id", _popIdentityConfig.GoogleClientID},
                {"client_secret", _popIdentityConfig.GoogleClientSecret},
                {"redirect_uri", "https://localhost:44353/home/callback"},
                {"grant_type", "authorization_code"}
            };
            var result = await client.PostAsync("https://www.googleapis.com/oauth2/v4/token", new FormUrlEncodedContent(values));
            var text = await result.Content.ReadAsStringAsync();
            var idToken = JObject.Parse(text).Root.SelectToken("id_token").ToString();
			var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(idToken);
            var sb = new StringBuilder();
            foreach (var item in token.Claims)
	            sb.Append($"{item.Type}: {item.Value}\r\n");
			// sub: unique ID, name, email
            return Content(sb.ToString());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
