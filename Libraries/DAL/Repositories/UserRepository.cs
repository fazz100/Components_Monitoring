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
	public class UserRepository : IUserRepository
	{
      private readonly DapperHelper<UserModel> _dapper;

		public UserRepository()
		{
         _dapper = new DapperHelper<UserModel>();
      }
      public List<UserModel> GetAll(string searchTerm=null)
		{
         try
         {

            string sql = @"
            select [id]
            ,username 
            ,[password]
            ,first_name 
            ,last_name
            ,created_date
            ,created_by
            ,updated_date
            ,updated_by
            ,is_deleted
            from [user] 
            where (@searchTerm is null or (@searchTerm is not null and 
               (
                  [username] like @searchTerm + '%'
                  or [first_name] like @searchTerm + '%'
                  or [last_name] like @searchTerm + '%'
                  or [last_name] + ', ' + [first_name] like @searchTerm + '%'
                  or [last_name] + ' ' + [first_name] like @searchTerm + '%'
                  or [first_name] + ' ' + [last_name] like @searchTerm + '%'
               )
            ))
            ";

            

            var model = _dapper.GetAll(sql, new { searchTerm= searchTerm });

            return model;
         }
         catch (Exception)
         {

            throw;
         }
      }


		public UserModel GetByUsername(string username)
		{

         try
         {

            string sql = @"
            select [id]
            ,username 
            ,[password] 
            ,first_name 
            ,last_name
            ,created_date
            ,created_by
            ,updated_date
            ,updated_by
            ,is_deleted
            from [user] 
            where username=@username
            ";

            var model = _dapper.Get(sql, new { username = username });

            return model;
         }
         catch (Exception)
         {

            throw;
         }
      }




      public UserModel GetById(string id)
      {

         try
         {

            string sql = @"
            select [id]
            ,username 
            ,[password] 
            ,first_name 
            ,last_name
            ,created_date
            ,created_by
            ,updated_date
            ,updated_by
            ,is_deleted
            from [user] 
            where id=@user_id
            ";

            var model = _dapper.Get(sql, new { user_id = id });

            return model;
         }
         catch (Exception)
         {

            throw;
         }
      }


      public void Insert(UserModel model)
      {

         try
         {
            string sql = @"
            insert into [user] (
            [id]
            ,username 
            ,[password] 
            ,first_name 
            ,last_name
            ,created_date
            ,created_by
            ,is_deleted
            )
            values
            (
            @id
            ,@username 
            ,@password
            ,@first_name 
            ,@last_name
            ,@created_date
            ,@created_by
            ,0
            );";

            _dapper.Execute(sql, model);
         }
         catch (Exception)
         {

            throw;
         }
      }


      public void Update(UserModel model)
      {

         try
         {

            string sql = @"
            update [user] set 
            [password] =@password
            ,first_name =@first_name 
            ,last_name=@last_name
            ,is_deleted=@is_deleted
            ,updated_date=@updated_date
            where id=@id";

            _dapper.Execute(sql, model);
         }
         catch (Exception)
         {

            throw;
         }
      }

   }
}
