using DAL.Helpers;
using DAL.Interfaces;
using ModelsLibrary.Models;
using System;
using System.Collections.Generic;

namespace DAL.Repositories
{
   public class ApplicationExceptionRepository : IApplicationExceptionRepository
   {
      private readonly DapperHelper<ApplicationExceptionsModel> _dapper;

      public ApplicationExceptionRepository()
      {
         _dapper = new DapperHelper<ApplicationExceptionsModel>();
      }

      public ApplicationExceptionsModel Get(string id)
      {
         try
         {
            string sql = @"SELECT 
                                ae.id
                                ,application_id
                                ,a.application_name
                                ,reason_for_exception
                                ,created_by
                                ,created_date
                                FROM application_exceptions ae
              inner join applications a on a.id=ae.application_id 
                                WHERE ae.id = @id";

            return _dapper.Get(sql, new { id = id });
         }
         catch (Exception)
         {
            throw;
         }
      }

      public List<ApplicationExceptionsModel> GetAll(string appName = null)
      {
         try
         {
            string sql = @"SELECT 
                                ae.id
                                ,ae.application_id
                                ,a.application_name
                                ,ae.reason_for_exception
                                ,ae.created_by
                                ,ae.created_date
                                FROM application_exceptions ae
              inner join applications a on a.id=ae.application_id 
               where (@appName is null or (@appName is not null and a.application_name like @appName + '%'))
                                ORDER BY created_date DESC";

            return _dapper.GetAll(sql, new {appName = appName });
         }
         catch (Exception)
         {
            throw;
         }
      }

      public void Insert(ApplicationExceptionsModel model)
      {
         try
         {
            string sql = @"INSERT INTO application_exceptions 
                                (
                                id 
                                ,application_id
                                ,reason_for_exception
                                ,created_by
                                ,created_date
                                ) 
                                VALUES 
                                (
                                @id 
                                ,@application_id
                                ,@reason_for_exception
                                ,@created_by
                                ,@created_date
                                )";

            _dapper.Execute(sql, model);
         }
         catch (Exception)
         {
            throw;
         }
      }

      public void Delete(string id)
      {
         try
         {
            string sql = "DELETE FROM application_exceptions WHERE id = @id";
            _dapper.Execute(sql, new { id = id });
         }
         catch (Exception)
         {
            throw;
         }
      }
   }
}