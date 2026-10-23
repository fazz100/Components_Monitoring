using DAL.Helpers;
using DAL.Interfaces;
using ModelsLibrary.Models;

namespace DAL.Repositories
{
   public class ApplicationGroupApplicationRepository : IApplicationGroupApplicationRepository
   {
      private readonly DapperHelper<ApplicationGroupApplicationModel> _dapper;

      public ApplicationGroupApplicationRepository()
      {
         _dapper = new DapperHelper<ApplicationGroupApplicationModel>();
      }

      public ApplicationGroupApplicationModel GetByApplicationId(string applicationId)
      {
         string sql = @"SELECT
                        id
                        ,application_group_id
                        ,application_id
                        FROM application_group_applications
                        WHERE application_id = @applicationId";

         return _dapper.Get(sql, new { applicationId = applicationId });
      }

      public void Insert(ApplicationGroupApplicationModel model)
      {
         string sql = @"INSERT INTO application_group_applications
                        (
                        id
                        ,application_group_id
                        ,application_id
                        )
                        VALUES
                        (
                        @Id
                        ,@Application_Group_Id
                        ,@Application_Id
                        )";

         _dapper.Execute(sql, model);
      }

      public void Update(ApplicationGroupApplicationModel model)
      {
         string sql = @"UPDATE application_group_applications SET
                        application_group_id = @Application_Group_Id
                        WHERE id = @Id";

         _dapper.Execute(sql, model);
      }

      public void DeleteByApplicationId(string applicationId)
      {
         string sql = "DELETE FROM application_group_applications WHERE application_id = @applicationId";
         _dapper.Execute(sql, new { applicationId = applicationId });
      }
   }
}
