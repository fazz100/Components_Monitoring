using ComponentsMonitoringAPI.BLL.Interfaces;
using ComponentsMonitoringAPI.BLL.Services;
using DAL.Interfaces;
using DAL.Repositories;
using Dg3.CommonLibraries;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using ModelsLibrary.Helpers;
using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ComponentsMonitoringAPI.Providers
{
   // Validates the requesting client against [api_clients] and the user's credentials against [user] (Argon2id + pepper)
   public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
   {
      private readonly IApiClientRepository _apiClientRepository;
      private readonly IUserRepository _userRepository;
      private readonly IUserService _userService;
      private static LogWriter LOGGER = new LogWriter();

      public ApplicationOAuthProvider()
      {
         _apiClientRepository = new ApiClientRepository();
         _userRepository = new UserRepository();
         _userService = new UserService();
      }

      // Only registered, active clients may request tokens (password and refresh_token grants)
      public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
      {
         string clientId;
         string clientSecret;

         if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
            context.TryGetFormCredentials(out clientId, out clientSecret);

         if (string.IsNullOrEmpty(clientId))
         {
            context.SetError("invalid_client", "client_id is required.");
            return Task.CompletedTask;
         }

         try
         {
            var client = _apiClientRepository.GetActiveByClientId(clientId);

            if (client == null)
            {
               context.SetError("invalid_client", "Unknown or inactive client.");
               return Task.CompletedTask;
            }

            // Confidential clients must also send a matching secret; public clients (e.g. the SPA) have no secret
            if (!string.IsNullOrEmpty(client.Client_Secret_Hash))
            {
               if (string.IsNullOrEmpty(clientSecret)
                  || !string.Equals(HashHelper.ComputeSha512Hash(clientSecret), client.Client_Secret_Hash, StringComparison.OrdinalIgnoreCase))
               {
                  context.SetError("invalid_client", "Invalid client secret.");
                  return Task.CompletedTask;
               }
            }

            context.Validated(clientId);
         }
         catch (Exception ex)
         {
            LOGGER.Error(ex);
            context.SetError("server_error", "An error occurred while validating the client.");
         }

         return Task.CompletedTask;
      }

      // Handle username & password authentication (grant_type=password)
      public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
      {
         try
         {
            var user = _userRepository.GetByUsername(context.UserName);

            if (user == null || user.IsDeleted || user.Password == null
               || !_userService.VerifyPassword(context.Password, Encoding.UTF8.GetString(user.Password)))
            {
               context.SetError("invalid_grant", "Invalid username or password.");
               return Task.CompletedTask;
            }

            context.Validated(CreateTicket(user, context.Options.AuthenticationType));
         }
         catch (Exception ex)
         {
            // Catch server exceptions so CORS headers remain intact and a proper OAuth error is returned
            LOGGER.Error(ex);
            context.SetError("server_error", "An error occurred during login.");
         }

         return Task.CompletedTask;
      }

      // Handle refresh token flow (grant_type=refresh_token); the ticket is rebuilt in ApplicationRefreshTokenProvider.ReceiveAsync
      public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
      {
         var newIdentity = new ClaimsIdentity(context.Ticket.Identity);
         var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
         context.Validated(newTicket);
         return Task.CompletedTask;
      }

      // Adds userName / userId / fullName to the /token JSON response
      public override Task TokenEndpoint(OAuthTokenEndpointContext context)
      {
         foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
         {
            context.AdditionalResponseParameters.Add(property.Key, property.Value);
         }
         return Task.CompletedTask;
      }

      public static AuthenticationTicket CreateTicket(UserModel user, string authenticationType)
      {
         var fullName = $"{user.FirstName} {user.LastName}".Trim();

         var identity = new ClaimsIdentity(authenticationType);
         identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
         identity.AddClaim(new Claim(ClaimTypes.Name, user.Username ?? string.Empty));
         identity.AddClaim(new Claim("FullName", fullName));

         var properties = new AuthenticationProperties(new Dictionary<string, string>
         {
            { "userName", user.Username ?? string.Empty },
            { "userId", user.Id },
            { "fullName", fullName }
         });

         return new AuthenticationTicket(identity, properties);
      }
   }
}
