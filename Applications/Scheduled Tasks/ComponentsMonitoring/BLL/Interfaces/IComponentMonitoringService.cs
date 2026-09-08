using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentsMonitoring.BLL.Interfaces
{
	public interface IComponentMonitoringService
	{
		void ProcessScheduledTasks();

		void ProcessWindowsServices();

		void ProcessWebsites();
	}
}
