using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ComponentsMonitoringAPI.BLL.Interfaces
{
	public interface IApplicationExceptionService
	{
		ApplicationExceptionsModel GetById(string id);
		List<ApplicationExceptionsModel> GetAllExceptions(string appName = null);
		bool AddException(ApplicationExceptionsModel model);
		bool RemoveException(string id);
	}
}