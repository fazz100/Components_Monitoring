using ComponentsMonitoring.BLL.Interfaces;
using ComponentsMonitoring.BLL.Services;
using Dg3.CommonLibraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentsMonitoring
{
	class Program
	{
		static IComponentMonitoringService _componentMonitoringService;
		static void Main(string[] args)
		{
         LogWriter logger = new LogWriter();
         _componentMonitoringService = new ComponentMonitoringService();

         if (args == null || args.Length == 0)
         {
            ShowUsage();
            return;
         }

         try
         {
            switch (args[0])
            {
               case "-schtask":
                  //scheduled tasks   
                  _componentMonitoringService.ProcessScheduledTasks();
                  break;
               case "-winsvc":
                  //win services
                  _componentMonitoringService.ProcessWindowsServices();
                  break;
               case "-web":
                  //websites                      
                  _componentMonitoringService.ProcessWebsites();
                  break;
               default:
                  ShowUsage();
                  return;
            }
         }
         catch (Exception e)
         {
            logger.Error(e);
            Console.WriteLine("ERROR: " + e.Message + e.InnerException + e.Source);
         }

      }

      static void ShowUsage()
      {
         Console.WriteLine("Components Monitoring\n(C) DGSI 2026");
         Console.WriteLine("Usage:   ComponentsMonitoring.exe -schtask | -winsvc | -web ");
         Console.WriteLine("-schtask: for scheduled tasks");
         Console.WriteLine("-winsvc: for windows services");
         Console.WriteLine("-web: for websites");
      }
   }
}
