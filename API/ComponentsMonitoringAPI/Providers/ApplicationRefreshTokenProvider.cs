using DAL.Interfaces;
using DAL.Repositories;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using ModelsLibrary.Helpers;
using ModelsLibrary.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ComponentsMonitoringAPI.Providers
{
   // Persists single-use refresh tokens in [refresh_tokens]; only the SHA-512 hash of the token is stored
   public class ApplicationRefreshTokenProvider : AuthenticationTokenProvider
   {
      private readonly IRefreshTokenRepository _refreshTokenRepository;
      private readonly IUserRepository _userRepository;

      public ApplicationRefreshTokenProvider()
      {
         _refreshTokenRepository = new RefreshTokenRepository();
         _userRepository = new UserRepository();
      }

      // Save refresh token to DB
      public override Task CreateAsync(AuthenticationTokenCreateContext context)
      {
         var userId = context.Ticket.Identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
         if (string.IsNullOrEmpty(userId))
            return Task.CompletedTask;

         var expiryInDays = WebConfigurationManager.AppSettings["refresh_token_expiry_in_days"];
         var rawToken = Guid.NewGuid().ToString("N");
         var issuedAt = DateTime.UtcNow;

         var token = new RefreshTokenModel
         {
            Id = Guid.NewGuid().ToString(),
            User_Id = userId,
            Token_Hash = HashHelper.ComputeSha512Hash(rawToken),
            Issued_At = issuedAt,
            Expires_At = issuedAt.AddDays(!string.IsNullOrEmpty(expiryInDays) ? int.Parse(expiryInDays) : 7),
            Is_Revoked = false
         };

         context.Ticket.Properties.IssuedUtc = token.Issued_At;
         context.Ticket.Properties.ExpiresUtc = token.Expires_At;

         _refreshTokenRepository.Insert(token);

         context.SetToken(rawToken);
         return Task.CompletedTask;
      }

      // Validate and single-use rotate the refresh token
      public override Task ReceiveAsync(AuthenticationTokenReceiveContext context)
      {
         var token = _refreshTokenRepository.GetActiveByHash(HashHelper.ComputeSha512Hash(context.Token));

         // Unknown, expired, already used, or used concurrently by another request -> no ticket -> invalid_grant
         if (token == null || !_refreshTokenRepository.Revoke(token.Id))
            return Task.CompletedTask;

         // Rebuild the ticket from the current user record so deleted users can no longer refresh
         var user = _userRepository.GetById(token.User_Id);
         if (user == null || user.IsDeleted)
            return Task.CompletedTask;

         var ticket = ApplicationOAuthProvider.CreateTicket(user, OAuthDefaults.AuthenticationType);
         ticket.Properties.IssuedUtc = new DateTimeOffset(DateTime.SpecifyKind(token.Issued_At, DateTimeKind.Utc));
         ticket.Properties.ExpiresUtc = new DateTimeOffset(DateTime.SpecifyKind(token.Expires_At, DateTimeKind.Utc));

         context.SetTicket(ticket);
         return Task.CompletedTask;
      }
   }
}
