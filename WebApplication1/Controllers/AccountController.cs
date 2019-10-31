using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Memory;

namespace WebApplication1.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        public const string OAuth2GrantTypeJwtBearer = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        public const string UserIdClaim = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        private readonly ILogger _logger;
        private readonly IMemoryCache _cache;

        public AccountController(ILogger<AccountController> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> Token()
        {
            var claims = HttpContext.User.Identity as ClaimsIdentity;
            var userId = User.FindFirst(UserIdClaim).Value;
            var userAccessToken = claims.Claims.Where(c => c.Type == "access_token").FirstOrDefault();

            // Check cache for SAML assertion
            var samlAssertionString = await _cache.GetOrCreateAsync<string>(userId, async item =>
            {
                string grant_type = OAuth2GrantTypeJwtBearer;
                string assertion = userAccessToken.Value;
                string client_id = Startup.ClientId;
                string client_secret = Startup.ClientSecret;
                string requested_token_use = "on_behalf_of";
                string request_token_type = "urn:ietf:params:oauth:token-type:saml2";

                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(Startup.AadInstance);

                var formContent = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("grant_type", grant_type),
                    new KeyValuePair<string, string>("assertion", assertion),
                    new KeyValuePair<string, string>("client_id", client_id),
                    new KeyValuePair<string, string>("client_secret", client_secret),
                    new KeyValuePair<string, string>("resource", Startup.Resource),
                    new KeyValuePair<string, string>("requested_token_use", requested_token_use),
                    new KeyValuePair<string, string>("requested_token_type", request_token_type)
                });

                var response = await httpClient.PostAsync($"{Startup.TenantId}/oauth2/token", formContent);
                var responseString = await response.Content.ReadAsStringAsync();
                var jToken = JObject.Parse(responseString).SelectToken("access_token");
                var base64SamlAssertion = jToken.Value<string>();
                var samlString = Base64UrlEncoder.Decode(base64SamlAssertion);

                // TODO: Set expiration
                //item.AbsoluteExpiration = new DateTimeOffset()

                return samlString;
            });

            // Display the SAML token
            if (!String.IsNullOrEmpty(samlAssertionString))
            {
                XElement x = XElement.Parse(samlAssertionString);
                ViewData["Token"] = x.ToString();
            }
            else
            {
                ViewData["Token"] = "Token not found. Try logging out and then logging back in again.";
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var redirectUrl = Url.Content("~/");

            // ++WS-Federation
            //return Challenge(
            //    new AuthenticationProperties { RedirectUri = redirectUrl },
            //    WsFederationDefaults.AuthenticationScheme);
            //return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, Saml2Defaults.Scheme);
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var redirectUrl = Url.Content("~/");
            //return SignOut(new AuthenticationProperties { RedirectUri = redirectUrl }, Saml2Defaults.Scheme);
            return SignOut(new AuthenticationProperties { RedirectUri = redirectUrl }, OpenIdConnectDefaults.AuthenticationScheme);

            // ++WS-Federation
            //return SignOut(
            //    new AuthenticationProperties { RedirectUri = redirectUrl },
            //    WsFederationDefaults.AuthenticationScheme);
        }
    }
}