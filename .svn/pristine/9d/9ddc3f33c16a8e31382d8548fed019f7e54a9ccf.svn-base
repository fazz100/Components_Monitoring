using ComponentsMonitoringExceptionsUpdater.BLL.Interfaces;
using DAL.Interfaces;
using DAL.Repositories;
using Dg3.CommonLibraries;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentsMonitoringExceptionsUpdater.BLL.Services
{
	public class ExceptionsUpdaterService : IExceptionsUpdaterService
	{
		private IApplicationExceptionRepository _applicationExceptionsRepository;
		private static LogWriter LOGGER = new LogWriter();

		public ExceptionsUpdaterService()
		{
			_applicationExceptionsRepository = new ApplicationExceptionRepository();
		}
		public void ProcessExceptions()
		{
         try
         {
            var daysSilenceDuration = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["days_silence_duration"])? int.Parse(ConfigurationManager.AppSettings["days_silence_duration"]) : 7;
            LOGGER.Info("Starting Application Exceptions Update");
            //get all the application exceptions
            var list = _applicationExceptionsRepository.GetAll();

            var appsToMonitor = new List<Tuple<string, string, string>>();
            foreach (var l in list)
            {
               //check the current date
               var duration = DateTime.UtcNow.Subtract(l.Created_Date);
               if (duration.Days >= daysSilenceDuration)
               {
                  LOGGER.Info("Removing " + l.Application_Name + " from exceptions list.");
                  _applicationExceptionsRepository.Delete(l.Id);
               }
            }


            LOGGER.Info("Application Exceptions Update Task Complete");
         }
         catch (Exception ex)
         {
            LOGGER.Info("ProcessExceptions has encountered an error. Exception: " + ex.Message + "; Inner Exception: " + (ex.InnerException == null ? "" : ex.InnerException.Message));

            LogWriter log = new LogWriter();
            log.Error(ex);
         }
      }
	}
}
