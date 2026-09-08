using DAL.Helpers;
using DAL.Interfaces;
using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
	public class UserSessionRepository : IUserSessionRepository
	{
      private readonly DapperHelper<UserSessionModel> _dapper;
		public UserSessionRepository()
		{
         _dapper = new DapperHelper<UserSessionModel>();
      }
      public void Insert(UserSessionModel model)
      {
         try
         {
            string sql = @"
            INSERT INTO [dbo].[user_session]
            (
            [id], 
            [user_id], 
            [token], 
            [issued_at], 
            [expires_at], 
            [is_revoked]
            )
            VALUES 
            (
            @id, 
            @user_id, 
            @token, 
            @issued_at, 
            @expires_at, 
            @is_revoked
            )";

            _dapper.Execute(sql, model);
         }
         catch (Exception ex)
         {

            throw;
         }
      }


      public UserSessionModel GetByToken(string token)
      {

         try
         {
            string sql = @"
            SELECT 
            [id]
            ,[user_id]
            ,[token]
            ,[issued_at]
            ,[expires_at]
            ,[is_revoked]
            FROM 
            [dbo].[user_session]
            WHERE [token] = @token";



            var model = _dapper.Get(sql, new { token = token });

            return model;
         }
         catch (Exception)
         {

            throw;
         }
      }



      public void LogoutSession(UserSessionModel model)
      {
         try
         {
            string sql = @"
            UPDATE [dbo].[user_session]
            SET is_revoked=1
            WHERE [token] = @token";

            var parameters = new SqlParameter[] {
            new SqlParameter("token", model.Token)
            ,new SqlParameter("is_revoked", 1)
            };

            _dapper.Execute(sql, model);
         }
         catch (Exception)
         {
            throw;
         }
      }

      public void ExtendSession(UserSessionModel model)
      {
         try
         {
            string sql = @"
            UPDATE [dbo].[user_session]
            SET expires_at=@expires_at
            ,is_revoked=0
            WHERE [token] = @token";

            _dapper.Execute(sql, model);
         }
         catch (Exception)
         {
            throw;
         }
      }
   }
}
