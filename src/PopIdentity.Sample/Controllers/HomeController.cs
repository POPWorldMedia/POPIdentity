using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopIdentity.Configuration;
using PopIdentity.Providers.Facebook;
using PopIdentity.Providers.Google;
using PopIdentity.Providers.Microsoft;
using PopIdentity.Providers.OAuth2;
using PopIdentity.Sample.Models;

namespace PopIdentity.Sample.Controllers
{
    public class HomeController : Controller
    {
	    private readonly IPopIdentityConfig _popIdentityConfig;
	    private readonly ILoginLinkFactory _loginLinkFactory;
	    private readonly IFacebookCallbackProcessor _facebookCallbackProcessor;
	    private readonly IGoogleCallbackProcessor _googleCallbackProcessor;
	    private readonly IStateHashingService _stateHashingService;
	    private readonly IMicrosoftCallbackProcessor _microsoftCallbackProcessor;
	    private readonly IOAuth2JwtCallbackProcessor _oAuth2JwtCallbackProcessor;

	    public HomeController(IPopIdentityConfig popIdentityConfig, ILoginLinkFactory loginLinkFactory, IFacebookCallbackProcessor facebookCallbackProcessor, IGoogleCallbackProcessor googleCallbackProcessor, IStateHashingService stateHashingService, IMicrosoftCallbackProcessor microsoftCallbackProcessor, IOAuth2JwtCallbackProcessor oAuth2JwtCallbackProcessor)
	    {
		    _popIdentityConfig = popIdentityConfig;
		    _loginLinkFactory = loginLinkFactory;
		    _facebookCallbackProcessor = facebookCallbackProcessor;
		    _googleCallbackProcessor = googleCallbackProcessor;
		    _stateHashingService = stateHashingService;
		    _microsoftCallbackProcessor = microsoftCallbackProcessor;
            _oAuth2JwtCallbackProcessor = oAuth2JwtCallbackProcessor;
	    }

	    public IActionResult Index()
	    {
            return View();
        }

	    [HttpPost]
	    public IActionResult ExternalLogin(string id)
	    {
		    var state = _stateHashingService.SetCookieAndReturnHash();
		    switch (id.ToLower())
		    {
				case "facebook":
					// This URL has to be specified in the Facebook developer console under "Valid OAuth Redirect URIs."
					var facebookRedirect = "https://localhost:5001/home/callbackfb";
					var facebookLink = _loginLinkFactory.GetLink(ProviderType.Facebook, facebookRedirect, state);
					return Redirect(facebookLink);
				case "google":
					// This URL has to specified in the Google Cloud console under Credentials -> OAuth 2.0 client ID's
					var googleRedirect = "https://localhost:5001/home/callbackgoogle";
					var googleLink = _loginLinkFactory.GetLink(ProviderType.Google, googleRedirect, state);
					return Redirect(googleLink);
				case "microsoft":
					// This URL has to specified in the Azure Portal under AD app registrations
					var msftRedirect = "https://localhost:5001/home/callbackmicrosoft";
					var msftLink = _loginLinkFactory.GetLink(ProviderType.Microsoft, msftRedirect, state);
					return Redirect(msftLink);
				case "msft":
					// This URL has to be specified as legal by whatever provider you're using
					var oauthRedirect = "https://localhost:5001/home/callbackoauth";
					var linkGenerator = new OAuth2LoginUrlGenerator();
					// choose the scope you're looking for
					var scopes = new List<string>(new[] {"openid", "email"});
					var oauthLink = linkGenerator.GetUrl(_popIdentityConfig.OAuth2LoginUrl, _popIdentityConfig.OAuth2ClientID, oauthRedirect, state, scopes);
					return Redirect(oauthLink);
				default: throw new NotImplementedException($"The external login \"{id}\" is not configured.");
		    }
		}

	    public async Task<IActionResult> CallbackOAuth()
	    {
		    var result = await _oAuth2JwtCallbackProcessor.VerifyCallback("https://localhost:5001/home/callbackoauth", _popIdentityConfig.OAuth2TokenUrl);
		    if (!result.IsSuccessful)
			    return Content(result.Message);
		    var list = $"id: {result.ResultData.ID}\r\nname: {result.ResultData.Name}\r\nemail: {result.ResultData.Email}";
		    return Content(list);
	    }

		public async Task<IActionResult> CallbackMicrosoft()
	    {
		    var result = await _microsoftCallbackProcessor.VerifyCallback("https://localhost:5001/home/callbackmicrosoft");
		    if (!result.IsSuccessful)
			    return Content(result.Message);
		    var list = $"id: {result.ResultData.ID}\r\nname: {result.ResultData.Name}\r\nemail: {result.ResultData.Email}";
		    return Content(list);
	    }

		public async Task<IActionResult> CallbackFB()
        {
			var result = await _facebookCallbackProcessor.VerifyCallback("https://localhost:5001/home/callbackfb");
			if (!result.IsSuccessful)
				return Content(result.Message);
			var list = $"id: {result.ResultData.ID}\r\nname: {result.ResultData.Name}\r\nemail: {result.ResultData.Email}";
			return Content(list);
        }

		public async Task<IActionResult> CallbackGoogle()
		{
			var result = await _googleCallbackProcessor.VerifyCallback("https://localhost:5001/home/callbackgoogle");
			if (!result.IsSuccessful)
				return Content(result.Message);
			var list = $"id: {result.ResultData.ID}\r\nname: {result.ResultData.Name}\r\nemail: {result.ResultData.Email}\r\ngiven_name: {result.Claims.FirstOrDefault(x => x.Type == "given_name")}\r\nfamily_name: {result.Claims.FirstOrDefault(x => x.Type == "family_name")}\r\npicture: {result.Claims.FirstOrDefault(x => x.Type == "picture")}";
			return Content(list);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
