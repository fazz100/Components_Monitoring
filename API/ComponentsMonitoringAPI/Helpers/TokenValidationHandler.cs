using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ComponentsMonitoringAPI.Helpers
{
   public class TokenValidationHandler : DelegatingHandler
   {
      


      
      protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
      {

        
         string reactAppUrl = WebConfigurationManager.AppSettings["reactAppUrl"];
         if (request.Method == HttpMethod.Options)
         {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("Access-Control-Allow-Origin", reactAppUrl);
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept, X-Api-Token, Authorization");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
            return response;
         }

         /*
         if (request.Method == HttpMethod.Options)
        return await base.SendAsync(request, cancellationToken);
         */


         IEnumerable<string> tokenValues;
         if (!request.Headers.TryGetValues("X-Api-Token", out tokenValues))
         {
            return request.CreateResponse(HttpStatusCode.Unauthorized, "Missing API token.");
         }

         var token = tokenValues.FirstOrDefault();

         if (string.IsNullOrEmpty(token) || !TokenStore.ValidTokens.Contains(token))
         {
            return request.CreateResponse(HttpStatusCode.Unauthorized, "Invalid or expired token.");
         }

         // Token is valid; continue to controller
         return await base.SendAsync(request, cancellationToken);
      }
      
   }
}