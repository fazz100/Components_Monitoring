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
                        ,aga.application_group_id
                        ,a.alert_pending
                        ,a.failure_detected_at
                        ,a.failure_details
                        from applications a
                        left join application_group_applications aga on aga.application_id = a.id
                        where a.id=@id";

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
                        ,aga.application_group_id
                        ,a.alert_pending
                        ,a.failure_detected_at
                        ,a.failure_details
                        ,g.application_group_name
                        ,g.is_deleted as application_group_is_deleted
                        from applications a
                        left join application_group_applications aga on aga.application_id = a.id
                        left join application_group g on g.id = aga.application_group_id
                        where
                        a.is_deleted=0
                        and (@type is null or (a.application_type=@type))
                        and (@includeExceptions=1 or (@includeExceptions=0 and
                        NOT EXISTS (SELECT 1 FROM application_exceptions e WHERE e.application_id = a.id
                           or (e.application_group_id is not null and e.application_group_id = aga.application_group_id))))
                        and (@includeInactiveGroups=1 or aga.application_group_id is null or g.is_deleted=0)
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
                        )";

            _dapper.Execute(sql, model);

         }
         catch (Exception)
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

      // Group membership is not written here: it lives in application_group_applications (ApplicationGroupApplicationRepository)
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

      // Used by the health checker only. Update() deliberately does not write these columns,
      // so saving an application from the dashboard can never wipe a pending alert.
      public void UpdateAlertState(ApplicationModel model)
      {
         string sql = @"UPDATE applications SET
                     working_status=@Working_Status
                     , alert_pending=@Alert_Pending
                     , failure_detected_at=@Failure_Detected_At
                     , failure_details=@Failure_Details
                     , updated_date=GETUTCDATE()
                           WHERE id=@Id";
         _dapper.Execute(sql, model);
      }

      // Failed applications waiting to be emailed by the alert dispatcher.
      // The grace period lets other failures in the same group be detected so they go out in one digest.
      public List<ApplicationModel> GetPendingAlerts(int gracePeriodMinutes)
      {
         string sql = @"Select
                     a.id
                     ,a.application_name
                     ,a.application_type
                     ,a.ip_address
                     ,a.url_or_app_name
                     ,a.[working_status]
                     ,aga.application_group_id
                     ,a.alert_pending
                     ,a.failure_detected_at
                     ,a.failure_details
                     ,g.application_group_name
                     ,g.is_deleted as application_group_is_deleted
                     ,cast(case when exists (SELECT 1 FROM application_exceptions e WHERE e.application_id = a.id
                           or (e.application_group_id is not null and e.application_group_id = aga.application_group_id))
                        then 1 else 0 end as bit) as is_silenced
                     from applications a
                     left join application_group_applications aga on aga.application_id = a.id
                     left join application_group g on g.id = aga.application_group_id
                     where a.working_status = 0
                     and a.alert_pending = 1
                     and a.is_deleted = 0
                     and (a.failure_detected_at is null or a.failure_detected_at <= DATEADD(minute, -@gracePeriodMinutes, GETUTCDATE()))
                     order by g.application_group_name, a.application_name";

         return _dapper.GetAll(sql, new { gracePeriodMinutes = gracePeriodMinutes });
      }

      // Used by ComponentsMonitoringAutoDiscovery to skip components that are already registered.
      // Soft-deleted rows are included on purpose: deleting a component on the dashboard opts it out of discovery.
      public List<ApplicationModel> GetAllByServer(string ipAddress)
      {
         string sql = @"Select
                     id
                     ,application_name
                     ,application_type
                     ,url_or_app_name
                     ,ip_address
                     ,is_deleted
                     from applications
                     where ip_address = @ipAddress";

         return _dapper.GetAll(sql, new { ipAddress = ipAddress });
      }

      public void ClearAlertPendingFlags(IEnumerable<string> applicationIds)
      {
         var ids = applicationIds?.ToList();
         if (ids == null || ids.Count == 0)
            return;

         string sql = @"UPDATE applications
                     SET alert_pending = 0, updated_date = GETUTCDATE()
                     WHERE id IN @ids";
         _dapper.Execute(sql, new { ids = ids });
      }
   }
}
