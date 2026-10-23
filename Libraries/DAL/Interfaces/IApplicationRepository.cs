using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
	public interface IApplicationRepository
	{
		ApplicationModel Get(string id);
		// includeInactiveGroups defaults to true so the monitoring checkers still check apps whose group was soft-deleted
		List<ApplicationModel> GetAll(string type = "", string appName = null, bool includeExceptions = false, bool includeInactiveGroups = true);
		void Insert(ApplicationModel model);
		void Update(ApplicationModel model);
		void Delete(string id, string updatedBy);

		// alert pipeline: health checker -> alert_pending -> ComponentsMonitoringAlertDispatcher
		void UpdateAlertState(ApplicationModel model);
		List<ApplicationModel> GetPendingAlerts(int gracePeriodMinutes);
		void ClearAlertPendingFlags(IEnumerable<string> applicationIds);

		// auto-discovery: every application on a server, including soft-deleted ones
		List<ApplicationModel> GetAllByServer(string ipAddress);
	}
}
