using ComponentsMonitoringAlertDispatcher.BLL.Interfaces;
using ComponentsMonitoringAlertDispatcher.BLL.Services;
using Dg3.CommonLibraries;
using System;

namespace ComponentsMonitoringAlertDispatcher
{
   class Program
   {
      static IAlertDispatcherService _alertDispatcherService;

      static void Main(string[] args)
      {
         LogWriter logger = new LogWriter();
         _alertDispatcherService = new AlertDispatcherService();

         try
         {
            _alertDispatcherService.DispatchAlerts();
         }
         catch (Exception e)
         {
            logger.Error(e);
            Console.WriteLine("ERROR: " + e.Message + e.InnerException + e.Source);
         }
      }
   }
}
