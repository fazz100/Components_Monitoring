using ComponentsMonitoringAPI.Providers;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Cors;
using System.Web.Http;

[assembly: OwinStartup(typeof(ComponentsMonitoringAPI.Startup))]

namespace ComponentsMonitoringAPI
{
   // OWIN startup pipeline: CORS -> OAuth -> Web API
   public class Startup
   {
      public void Configuration(IAppBuilder app)
      {
         // 1. Enable OWIN CORS middleware FIRST so /token and preflight requests get CORS headers
         app.UseCors(CreateCorsOptions());

         // 2. Configure OAuth middleware
         ConfigureOAuth(app);

         // 3. Configure Web API using GlobalConfiguration (WebApiConfig.Register is already called from Global.asax)
         HttpConfiguration config = GlobalConfiguration.Configuration;
         app.UseWebApi(config);
      }

      private CorsOptions CreateCorsOptions()
      {
         // Only the React dashboard origin is allowed
         var policy = new CorsPolicy
         {
            AllowAnyHeader = true,
            AllowAnyMethod = true,
            SupportsCredentials = true
         };
         policy.Origins.Add(WebConfigurationManager.AppSettings["reactAppUrl"]);

         return new CorsOptions
         {
            PolicyProvider = new CorsPolicyProvider
            {
               PolicyResolver = request => Task.FromResult(policy)
            }
         };
      }

      private void ConfigureOAuth(IAppBuilder app)
      {
         var allowInsecureHttp = WebConfigurationManager.AppSettings["oauth_allow_insecure_http"];
         var accessTokenExpiry = WebConfigurationManager.AppSettings["access_token_expiry_in_minutes"];

         OAuthAuthorizationServerOptions oAuthServerOptions = new OAuthAuthorizationServerOptions()
         {
            AllowInsecureHttp = !string.IsNullOrEmpty(allowInsecureHttp) && bool.Parse(allowInsecureHttp), // HTTPS is enforced unless explicitly allowed (dev only)
            TokenEndpointPath = new PathString("/token"),
            AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(!string.IsNullOrEmpty(accessTokenExpiry) ? int.Parse(accessTokenExpiry) : 15),
            Provider = new ApplicationOAuthProvider(),
            RefreshTokenProvider = new ApplicationRefreshTokenProvider()
         };

         app.UseOAuthAuthorizationServer(oAuthServerOptions);
         app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
      }
   }
}
