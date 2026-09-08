using ComponentsMonitoringExceptionsUpdater.BLL.Interfaces;
using ComponentsMonitoringExceptionsUpdater.BLL.Services;
using Dg3.CommonLibraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentsMonitoringExceptionsUpdater
{
	public class Program
	{
      static IExceptionsUpdaterService _exceptionsUpdaterService;
      static void Main(string[] args)
      {
         LogWriter logger = new LogWriter();
         _exceptionsUpdaterService = new ExceptionsUpdaterService();


         try
         {
            _exceptionsUpdaterService.ProcessExceptions();
         }
         catch (Exception e)
         {
            logger.Error(e);
            Console.WriteLine("ERROR: " + e.Message + e.InnerException + e.Source);
         }

      }
   }
}
