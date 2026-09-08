using DAL.Interfaces;
using DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace ComponentsMonitoringAPI.Attributes
{
	public class TokenAuthorizeAttribute : AuthorizeAttribute
   {
      protected override bool IsAuthorized(HttpActionContext actionContext)
      {
         //return true;
         IUserSessionRepository _sessionRepository = new UserSessionRepository();

         var authHeader = actionContext.Request.Headers.Authorization;
         if (authHeader == null || authHeader.Scheme != "Bearer")
            return false;

         var token = authHeader.Parameter;
         if (string.IsNullOrEmpty(token))
            return false;

         var userSession = _sessionRepository.GetByToken(token);

         if (userSession==null)
            return false; // No session found

         var expiresAt = userSession.Expires_At;
         var isRevoked = userSession.Is_Revoked;

         if (isRevoked || expiresAt < DateTime.UtcNow)
         {
            _sessionRepository.LogoutSession(userSession);
            return false;
         }

         var expiryInSeconds = int.Parse(WebConfigurationManager.AppSettings["SessionExpiryInSeconds"]);
         userSession.Expires_At = DateTime.UtcNow.AddSeconds(expiryInSeconds);

         _sessionRepository.ExtendSession(userSession);

         actionContext.Request.Properties["UserSession"] = userSession;
         actionContext.Request.Properties["CurrentUserId"] = userSession.User_Id;

         return true; // Token valid
      }

      protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
      {
         actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, new
         {
            Message = "Unauthorized: Invalid or expired token."
         });
      }
   }
}