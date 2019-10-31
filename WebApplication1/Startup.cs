using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class Startup
    {
        private IHostingEnvironment _hostingEnvironment;

        public static string AadInstance = "https://login.microsoftonline.com/";
        public static string AppIdUri = "api://45d480b9-7ef9-41cb-93a2-4c176039665b";
        public static string ClientId = "45d480b9-7ef9-41cb-93a2-4c176039665b";
        public static string Tenant = "jasonvenemagmail.onmicrosoft.com";
        public static string ClientSecret = ""; // Client secret for this app from AAD portal
        public static string TenantId = "bce15b5e-3b8e-47c8-b79a-cd02f8002a4c";
        public static string Authority = AadInstance + Tenant;
        public static string Resource = ""; // Azure AD App ID URI for app to connect to

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();

            _hostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ClientSecret = Configuration["AzureAd:ClientSecret"];
            Resource = Configuration["AzureAd:Resource"];

            services
               .AddAuthentication(sharedOptions =>

               {
                   sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                   sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                   sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
               })
               .AddOpenIdConnect(options =>
               {
                   options.Authority = Authority;
                   options.ClientId = ClientId;
                   options.ClientSecret = ClientSecret;
                   options.SaveTokens = true;
                   //options.ResponseType = "code id_token";

                   //    options.Events.OnAuthorizationCodeReceived = async ctx =>
                   //    {
                   //        HttpRequest request = ctx.HttpContext.Request;
                   //        //We need to also specify the redirect URL used
                   //        string currentUri = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path);
                   //        //Credentials for app itself
                   //        var credential = new ClientCredential(ctx.Options.ClientId, ctx.Options.ClientSecret);

                   //        //Construct token cache
                   //        ITokenCacheFactory cacheFactory = ctx.HttpContext.RequestServices.GetRequiredService<ITokenCacheFactory>();
                   //        TokenCache cache = cacheFactory.CreateForUser(ctx.Principal);

                   //        var authContext = new AuthenticationContext(ctx.Options.Authority, cache);

                   //        //Get token for Microsoft Graph API using the authorization code
                   //        AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(
                   //            ctx.ProtocolMessage.Code, new Uri(currentUri), credential, Resource);

                   //        //Tell the OIDC middleware we got the tokens, it doesn't need to do anything
                   //        ctx.HandleCodeRedemption(result.AccessToken, result.IdToken);
                   //    };

                   options.Events.OnTokenValidated = context =>
                   {
                       var accessToken = context.SecurityToken as JwtSecurityToken;

                       if (accessToken != null)
                       {
                           var identity = context.Principal.Identity as ClaimsIdentity;
                           if (identity != null)
                           {
                               identity.AddClaim(new Claim("access_token", accessToken.RawData));
                           }
                       }

                       return Task.FromResult(0);
                   };
               })
               .AddCookie();

            services.AddMvc();
            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();
        }
    }
}