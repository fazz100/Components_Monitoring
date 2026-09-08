using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentsMonitoringAPI.BLL.Interfaces
{
	public interface IApplicationService
	{
		List<ApplicationModel> GetApplications(string type, string appName = null, bool includeExceptions = true);
		ApplicationModel GetById(string id);
		void Create(ApplicationModel model);
		void Update(ApplicationModel model);
		void Delete(string id, string user);
	}
}
