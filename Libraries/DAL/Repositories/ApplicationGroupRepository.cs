using DAL.Helpers;
using DAL.Interfaces;
using ModelsLibrary.Models;
using System.Collections.Generic;

namespace DAL.Repositories
{
   public class ApplicationGroupRepository : IApplicationGroupRepository
   {
      private readonly DapperHelper<ApplicationGroupModel> _dapper;

      public ApplicationGroupRepository()
      {
         _dapper = new DapperHelper<ApplicationGroupModel>();
      }

      public ApplicationGroupModel Get(string id)
      {
         string sql = @"SELECT
                        id
                        ,application_group_name
                        ,[description]
                        ,created_by
                        ,created_date
                        ,updated_by
                        ,updated_date
                        ,is_deleted
                        FROM application_group
                        WHERE id = @id";

         return _dapper.Get(sql, new { id = id });
      }

      public List<ApplicationGroupModel> GetAll(string searchTerm = null)
      {
         string sql = @"SELECT
                        id
                        ,application_group_name
                        ,[description]
                        ,created_by
                        ,created_date
                        ,updated_by
                        ,updated_date
                        ,is_deleted
                        FROM application_group
                        WHERE is_deleted = 0
                        and (@searchTerm is null or
                        (
                           application_group_name like @searchTerm + '%'
                           or [description] like @searchTerm + '%'
                        ))
                        ORDER BY application_group_name";

         return _dapper.GetAll(sql, new { searchTerm = searchTerm });
      }

      public ApplicationGroupModel GetByName(string name)
      {
         string sql = @"SELECT
                        id
                        ,application_group_name
                        ,[description]
                        ,created_by
                        ,created_date
                        ,updated_by
                        ,updated_date
                        ,is_deleted
                        FROM application_group
                        WHERE is_deleted = 0 and application_group_name = @name";

         return _dapper.Get(sql, new { name = name });
      }

      public void Insert(ApplicationGroupModel model)
      {
         string sql = @"INSERT INTO application_group
                        (
                        id
                        ,application_group_name
                        ,[description]
                        ,created_by
                        ,created_date
                        ,is_deleted
                        )
                        VALUES
                        (
                        @Id
                        ,@Application_Group_Name
                        ,@Description
                        ,@Created_By
                        ,@Created_Date
                        ,0
                        )";

         _dapper.Execute(sql, model);
      }

      public void Update(ApplicationGroupModel model)
      {
         string sql = @"UPDATE application_group SET
                        application_group_name = @Application_Group_Name
                        ,[description] = @Description
                        ,updated_by = @Updated_By
                        ,updated_date = @Updated_Date
                        WHERE id = @Id";

         _dapper.Execute(sql, model);
      }

      public void Delete(string id, string updatedBy)
      {
         string sql = "UPDATE application_group SET is_deleted=1, updated_by=@updatedBy, updated_date=GETUTCDATE() WHERE id=@id";
         _dapper.Execute(sql, new { id, updatedBy });
      }
   }
}
