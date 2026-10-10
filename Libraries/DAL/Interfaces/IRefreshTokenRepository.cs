using ModelsLibrary.Models;

namespace DAL.Interfaces
{
   public interface IRefreshTokenRepository
   {
      void Insert(RefreshTokenModel model);
      RefreshTokenModel GetActiveByHash(string tokenHash);
      // returns false if the token was already revoked (e.g. a concurrent refresh used it first)
      bool Revoke(string id);
      void RevokeByHash(string tokenHash, string userId);
   }
}
