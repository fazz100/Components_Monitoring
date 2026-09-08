using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentsMonitoringAPI.BLL.Interfaces
{
	public interface IApplicationDatabaseService
	{
		bool TestDatabaseConnection(string connectionString);

		bool SaveDatabase(ApplicationDatabaseModel model);
		bool UpdateDatabase(ApplicationDatabaseModel model);
		bool DeleteDatabase(string Id);
	}
}
