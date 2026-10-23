using ComponentsMonitoringAutoDiscovery.BLL.Interfaces;
using ComponentsMonitoringAutoDiscovery.BLL.Services;
using Dg3.CommonLibraries;
using System;

namespace ComponentsMonitoringAutoDiscovery
{
   class Program
   {
      static IAutoDiscoveryService _autoDiscoveryService;

      static void Main(string[] args)
      {
         LogWriter logger = new LogWriter();
         _autoDiscoveryService = new AutoDiscoveryService();

         try
         {
            _autoDiscoveryService.DiscoverComponents();
         }
         catch (Exception e)
         {
            logger.Error(e);
            Console.WriteLine("ERROR: " + e.Message + e.InnerException + e.Source);
         }
      }
   }
}
