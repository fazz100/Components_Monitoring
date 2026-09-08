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
	public class ApplicationDatabaseRepository: IApplicationDatabaseRepository
	{
		private readonly DapperHelper<ApplicationDatabaseModel> _dapper;
		private DapperHelper2<int> _dapper2;

		public ApplicationDatabaseRepository()
		{
			_dapper = new DapperHelper<ApplicationDatabaseModel>();
			
		}

		public List<ApplicationDatabaseModel> GetByAppId(string appId)
		{
			try
			{
				string sql = "SELECT * FROM application_databases WHERE application_id = @appId";
				return _dapper.GetAll(sql, new { appId = appId });
			}
			catch (Exception)
			{
				throw;
			}
		}

		public bool TestDatabaseConnection(string connectionString)
		{
			try
			{
				_dapper2 = new DapperHelper2<int>(connectionString);

				string sql = "SELECT 1";
				var n=_dapper2.Get(sql, new { });
				return true;
			}
			catch(Exception)
			{
				throw;
			}
		}

		public void Insert(ApplicationDatabaseModel model)
		{

			try
			{
				string sql = @"INSERT INTO [application_databases] 
                          (id, app_database_name, description, connection_string, application_id, created_by, created_date) 
                          VALUES (@Id, @App_Database_Name, @Description, @Connection_String, @Application_Id, @Created_By, @Created_Date)";

				_dapper.Execute(sql, model);
			}
			catch (Exception)
			{
				throw;
			}

		}

		public void Update(ApplicationDatabaseModel model)
		{

			try
			{
				string sql = @"UPDATE [application_databases] 
					set app_database_name = @App_Database_Name
					,description=@Description
					,connection_string =@Connection_String
					where id=@Id";

				_dapper.Execute(sql, model);
			}
			catch (Exception)
			{
				throw;
			}

		}

		public void Delete(string Id)
		{
			try
			{
				string sql = "DELETE FROM [application_databases] WHERE id = @Id";
				_dapper.Execute(sql, new { Id = Id });
			}
			catch (Exception)
			{
				throw;
			}
		}

		public void DeleteByAppId(string appId)
		{
			try
			{
				string sql = "DELETE FROM [application_databases] WHERE application_id = @appId";
				_dapper.Execute(sql, new { appId = appId });
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
