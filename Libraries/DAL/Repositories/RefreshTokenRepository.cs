using DAL.Helpers;
using DAL.Interfaces;
using ModelsLibrary.Models;

namespace DAL.Repositories
{
   public class RefreshTokenRepository : IRefreshTokenRepository
   {
      private readonly DapperHelper<RefreshTokenModel> _dapper;

      public RefreshTokenRepository()
      {
         _dapper = new DapperHelper<RefreshTokenModel>();
      }

      public void Insert(RefreshTokenModel model)
      {
         string sql = @"
            INSERT INTO [dbo].[refresh_tokens]
            (
            [id]
            ,[user_id]
            ,[token_hash]
            ,[issued_at]
            ,[expires_at]
            ,[is_revoked]
            )
            VALUES
            (
            @Id
            ,@User_Id
            ,@Token_Hash
            ,@Issued_At
            ,@Expires_At
            ,@Is_Revoked
            )";

         _dapper.Execute(sql, model);
      }

      public RefreshTokenModel GetActiveByHash(string tokenHash)
      {
         string sql = @"
            SELECT
            [id]
            ,[user_id]
            ,[token_hash]
            ,[issued_at]
            ,[expires_at]
            ,[is_revoked]
            FROM [dbo].[refresh_tokens]
            WHERE [token_hash] = @tokenHash
            and [is_revoked] = 0
            and [expires_at] > GETUTCDATE()";

         return _dapper.Get(sql, new { tokenHash = tokenHash });
      }

      public bool Revoke(string id)
      {
         string sql = "UPDATE [dbo].[refresh_tokens] SET [is_revoked] = 1 WHERE [id] = @id and [is_revoked] = 0";
         return _dapper.Execute(sql, new { id = id }) == 1;
      }

      public void RevokeByHash(string tokenHash, string userId)
      {
         string sql = "UPDATE [dbo].[refresh_tokens] SET [is_revoked] = 1 WHERE [token_hash] = @tokenHash and [user_id] = @userId";
         _dapper.Execute(sql, new { tokenHash = tokenHash, userId = userId });
      }
   }
}
