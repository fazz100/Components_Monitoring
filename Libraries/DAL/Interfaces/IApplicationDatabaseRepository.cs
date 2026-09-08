using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
	public interface IApplicationDatabaseRepository
	{
		List<ApplicationDatabaseModel> GetByAppId(string appId);
		bool TestDatabaseConnection(string connectionString);
		void Insert(ApplicationDatabaseModel model);
		void Update(ApplicationDatabaseModel model);
		void Delete(string Id);
		void DeleteByAppId(string appId);
	}
}
