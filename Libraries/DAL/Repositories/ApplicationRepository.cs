using DAL.Helpers;
using DAL.Interfaces;
using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
	public class ApplicationRepository: IApplicationRepository
	{
      private readonly DapperHelper<ApplicationModel> _dapper;

      public ApplicationRepository()
      {
         _dapper = new DapperHelper<ApplicationModel>();
      }

      public ApplicationModel Get(string id)
      {
         try
         {
            string sql = @"select 
                        id 
                        ,application_name 
                        ,[description] 
                        ,application_type 
                        ,ip_address
                        ,url_or_app_name
                        ,[working_status]
                        ,is_enabled
                        ,service_status 
                        ,max_allowed_age
                        ,created_date 
                        ,created_by 
                        ,updated_date 
                        ,updated_by
                        ,is_deleted
                        ,application_group_id
                        from applications
                        where id=@id";

            var model = _dapper.Get(sql, new { id = id });

            return model;
         }
         catch (Exception)
         {

            throw;
         }
      }

      public List<ApplicationModel> GetAll(string type, string appName=null, bool includeExceptions=false, bool includeInactiveGroups=true)
      {
         try
         {
            string sql = @"Select
                        a.id
                        ,a.application_name
                        ,a.[description]
                        ,a.application_type
                        ,a.ip_address
                        ,a.url_or_app_name
                        ,a.[working_status]
                        ,a.is_enabled
                        ,a.service_status
                        ,a.max_allowed_age
                        ,a.created_date
                        ,a.created_by
                        ,a.updated_date
                        ,a.updated_by
                        ,a.is_deleted
                        ,a.application_group_id
                        ,g.application_group_name
                        ,g.is_deleted as application_group_is_deleted
                        from applications a
                        left join application_group g on g.id = a.application_group_id
                        where
                        a.is_deleted=0
                        and (@type is null or (a.application_type=@type))
                        and (@includeExceptions=1 or (@includeExceptions=0 and
                        NOT EXISTS (SELECT 1 FROM application_exceptions e WHERE e.application_id = a.id)))
                        and (@includeInactiveGroups=1 or a.application_group_id is null or g.is_deleted=0)
                        and (@appName is null or (@appName is not null and
                        (
                           a.application_name like @appName + '%'
                           or a.[description] like @appName + '%'
                           or a.[ip_address] like @appName + '%'
                           or a.[url_or_app_name] like @appName + '%'
                        )
                        ))";

            var model = _dapper.GetAll(sql, new { type = type, appName = appName, includeExceptions = includeExceptions, includeInactiveGroups = includeInactiveGroups });

            return model;

         }
         catch (Exception)
         {

            throw;
         }
      }

      
      public void Insert(ApplicationModel model)
      {
         try
         {
            string sql = @"insert into applications 
                        (
                        id 
                        ,application_name
                        ,[description]
                        ,application_type
                        ,ip_address
                        ,url_or_app_name
                        ,is_enabled
                        ,working_status
                        ,service_status
                        ,created_date
                        ,created_by
                        ,application_group_id
                        )
                        values
                        (
                        @id 
                        ,@application_name
                        ,@description
                        ,@application_type
                        ,@ip_address
                        ,@url_or_app_name
                        ,@is_enabled
                        ,@working_status
                        ,@service_status
                        ,@created_date
                        ,@created_by
                        ,@application_group_id
                        )";

            _dapper.Execute(sql, model);

         }
         catch (Exception ex)
         {

            throw;
         }
      }
      /*
      public void Update(ApplicationModel model)
      {
         try
         {
            string sql = @"update students set id=@id,first_name=@first_name,middle_name=@middle_name,last_name=@last_name,age=@age 
                               where id=@id";

            _dapper.Execute(
                sql,
                new
                {
                   id = model.Id,
                   first_name = model.First_Name,
                   middle_name = model.Middle_Name,
                   last_name = model.Last_Name,
                   age = model.Age
                });

         }
         catch (Exception)
         {

            throw;
         }
      }
      */

      public void Update(ApplicationModel model)
      {
         string sql = @"UPDATE applications SET 
                     application_name=@Application_Name
                     , description=@Description
                     ,application_type=@Application_Type
                     ,working_status=@Working_Status
                     ,service_status=@Service_Status
                     , ip_address=@IP_Address
                     , url_or_app_name=@URL_Or_App_Name
                     , is_enabled=@Is_Enabled
                     , application_group_id=@Application_Group_Id
                     , updated_date=@Updated_Date
                     , updated_by=@Updated_By 
                           WHERE id=@Id";
         _dapper.Execute(sql, model);
      }

      public void Delete(string id, string updatedBy)
      {
         string sql = "UPDATE applications SET is_deleted=1, updated_by=@updatedBy, updated_date=GETDATE() WHERE id=@id";
         _dapper.Execute(sql, new { id, updatedBy });
      }
   }
}
