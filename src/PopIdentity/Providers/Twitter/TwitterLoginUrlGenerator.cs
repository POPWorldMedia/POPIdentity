using System.Web;

namespace PopIdentity.Providers.Twitter
{
    public class TwitterLoginUrlGenerator
    {
        public string GetUrl(string baseUrl, string clientID, string redirectUrl, string state)
        {
            var urlEncodedRedirect = HttpUtility.UrlEncode(redirectUrl);
            var urlEncodedState = HttpUtility.UrlEncode(state);
            var link = $"{baseUrl}?client_id={clientID}&redirect_uri={urlEncodedRedirect}&state={urlEncodedState}&response_type=code&scope=https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.email+https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.profile+openid+email+profile";
            return link;
        }
    }
}