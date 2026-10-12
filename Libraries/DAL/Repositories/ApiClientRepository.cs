using DAL.Helpers;
using DAL.Interfaces;
using ModelsLibrary.Models;

namespace DAL.Repositories
{
   public class ApiClientRepository : IApiClientRepository
   {
      private readonly DapperHelper<ApiClientModel> _dapper;

      public ApiClientRepository()
      {
         _dapper = new DapperHelper<ApiClientModel>();
      }

      public ApiClientModel GetActiveByClientId(string clientId)
      {
         string sql = @"
            SELECT
            [id]
            ,[client_name]
            ,[client_id]
            ,[client_secret_hash]
            ,[is_active]
            ,[created_date]
            FROM [dbo].[api_clients]
            WHERE [client_id] = @clientId
            and [is_active] = 1";

         return _dapper.Get(sql, new { clientId = clientId });
      }
   }
}
