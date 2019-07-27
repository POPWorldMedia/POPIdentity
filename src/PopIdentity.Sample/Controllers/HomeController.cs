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
	    private readonly IOAuth2CallbackProcessor<GitHubResult> _oAuth2CallbackProcessor;

	    public HomeController(IPopIdentityConfig popIdentityConfig, ILoginLinkFactory loginLinkFactory, IFacebookCallbackProcessor facebookCallbackProcessor, IGoogleCallbackProcessor googleCallbackProcessor, IStateHashingService stateHashingService, IMicrosoftCallbackProcessor microsoftCallbackProcessor, IOAuth2CallbackProcessor<GitHubResult> oAuth2CallbackProcessor)
	    {
		    _popIdentityConfig = popIdentityConfig;
		    _loginLinkFactory = loginLinkFactory;
		    _facebookCallbackProcessor = facebookCallbackProcessor;
		    _googleCallbackProcessor = googleCallbackProcessor;
		    _stateHashingService = stateHashingService;
		    _microsoftCallbackProcessor = microsoftCallbackProcessor;
		    _oAuth2CallbackProcessor = oAuth2CallbackProcessor;
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
					var facebookRedirect = "https://localhost:44353/home/callbackfb";
					var facebookLink = _loginLinkFactory.GetLink(ProviderType.Facebook, facebookRedirect, state);
					return Redirect(facebookLink);
				case "google":
					// This URL has to specified in the Google Cloud console under Credentials -> OAuth 2.0 client ID's
					var googleRedirect = "https://localhost:44353/home/callbackgoogle";
					var googleLink = _loginLinkFactory.GetLink(ProviderType.Google, googleRedirect, state);
					return Redirect(googleLink);
				case "microsoft":
					// This URL has to specified in the Azure Portal under AD app registrations
					var msftRedirect = "https://localhost:44353/home/callbackmicrosoft";
					var msftLink = _loginLinkFactory.GetLink(ProviderType.Microsoft, msftRedirect, state);
					return Redirect(msftLink);
				case "msft":
					var oauthRedirect = "https://localhost:44353/home/callbackoauth";
					var linkGenerator = new OAuth2LoginUrlGenerator();
					// choose the claims you're looking for
					var oauthClaims = new List<string>(new[] {"openid", "email"});
					var oauthLink = linkGenerator.GetUrl(_popIdentityConfig.OAuth2LoginUrl, _popIdentityConfig.OAuth2ClientID, oauthRedirect, state, oauthClaims);
					return Redirect(oauthLink);
				default: throw new NotImplementedException($"The external login \"{id}\" is not configured.");
		    }
		}

	    public async Task<IActionResult> CallbackOAuth()
	    {
		    var result = await _oAuth2CallbackProcessor.VerifyCallback("https://localhost:44353/home/callbackoauth",
			    c =>
			    {
				    var claims = c.ToList();
				    return new GitHubResult
				    {
						ID = claims.FirstOrDefault(x => x.Type == "sub")?.Value,
					    Email = claims.FirstOrDefault(x => x.Type == "email")?.Value,
						Name = claims.FirstOrDefault(x => x.Type == "name")?.Value
				    };
			    },
			    _popIdentityConfig.OAuth2TokenUrl);
		    if (!result.IsSuccessful)
			    return Content(result.Message);
		    var list = $"id: {result.ResultData.ID}\r\nname: {result.ResultData.Name}\r\nemail: {result.ResultData.Email}";
		    return Content(list);
	    }

		public async Task<IActionResult> CallbackMicrosoft()
	    {
		    var result = await _microsoftCallbackProcessor.VerifyCallback("https://localhost:44353/home/callbackmicrosoft");
		    if (!result.IsSuccessful)
			    return Content(result.Message);
		    var list = $"id: {result.ResultData.ID}\r\nname: {result.ResultData.Name}\r\nemail: {result.ResultData.Email}";
		    return Content(list);
	    }

		public async Task<IActionResult> CallbackFB()
        {
			var result = await _facebookCallbackProcessor.VerifyCallback("https://localhost:44353/home/callbackfb");
			if (!result.IsSuccessful)
				return Content(result.Message);
			var list = $"id: {result.ResultData.ID}\r\nname: {result.ResultData.Name}\r\nemail: {result.ResultData.Email}";
			return Content(list);
        }

		public async Task<IActionResult> CallbackGoogle()
		{
			var result = await _googleCallbackProcessor.VerifyCallback("https://localhost:44353/home/callbackgoogle");
			if (!result.IsSuccessful)
				return Content(result.Message);
			var list = $"id: {result.ResultData.ID}\r\nname: {result.ResultData.Name}\r\nemail: {result.ResultData.Email}\r\ngiven_name: {result.ResultData.GivenName}\r\nfamily_name: {result.ResultData.FamilyName}\r\npicture: {result.ResultData.Picture}";
			return Content(list);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
