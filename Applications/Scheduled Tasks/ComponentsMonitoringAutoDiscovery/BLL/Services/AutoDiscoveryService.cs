using ComponentsMonitoringAutoDiscovery.BLL.Interfaces;
using DAL.Interfaces;
using DAL.Repositories;
using Dg3.CommonLibraries;
using Microsoft.Win32.TaskScheduler;
using ModelsLibrary.Models;
using ModelsLibrary.Models.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using WinTask = Microsoft.Win32.TaskScheduler.Task;

namespace ComponentsMonitoringAutoDiscovery.BLL.Services
{
   // Scans the target servers for DG3 Windows services and scheduled tasks and registers the new ones
   // as standalone applications (no application group), so the health checker starts monitoring them
   public class AutoDiscoveryService : IAutoDiscoveryService
   {
      private const string MATCH_TEXT = "dg3"; // compared case-insensitively, so every [DdGg]3 variation matches
      private const string WINDOWS_SERVICE = "windows service";
      private const string SCHEDULED_TASK = "scheduled task";

      private IApplicationRepository _applicationRepository;
      private static LogWriter LOGGER = new LogWriter();
      private int _registeredCount;

      public AutoDiscoveryService()
      {
         _applicationRepository = new ApplicationRepository();
      }

      public void DiscoverComponents()
      {
         try
         {
            LOGGER.Info("Starting Component Auto-Discovery");
            _registeredCount = 0;

            var servers = GetTargetServers();
            if (servers.Count == 0)
            {
               LOGGER.Info("No target servers configured in target_servers. Component Auto-Discovery Complete");
               return;
            }

            foreach (var server in servers)
            {
               try
               {
                  LOGGER.Info($"Scanning server {server}");

                  // every application already registered on this server, including deleted ones
                  var existing = _applicationRepository.GetAllByServer(server);

                  DiscoverWindowsServices(server, existing);
                  DiscoverScheduledTasks(server, existing);
               }
               catch (Exception ex)
               {
                  LOGGER.Info($"DiscoverComponents encountered an error while scanning server {server}: {ex.Message}");
                  LogWriter log = new LogWriter();
                  log.Error(ex);
               }
            }

            LOGGER.Info($"{_registeredCount} new component(s) registered");
            LOGGER.Info("Component Auto-Discovery Complete");
         }
         catch (Exception ex)
         {
            LOGGER.Info("DiscoverComponents has encountered an error. Exception: " + ex.Message + "; Inner Exception: " + (ex.InnerException == null ? "" : ex.InnerException.Message));
            LogWriter log = new LogWriter();
            log.Error(ex);
         }
      }

      private void DiscoverWindowsServices(string serverName, List<ApplicationModel> existing)
      {
         try
         {
            LOGGER.Info($"Discovering Windows services on server {serverName}");

            // Passing the server name scans it remotely
            foreach (ServiceController sc in ServiceController.GetServices(serverName))
            {
               using (sc)
               {
                  try
                  {
                     if (!Matches(sc.ServiceName) && !Matches(sc.DisplayName))
                        continue;

                     // the health checker opens the service by its service name
                     if (IsRegistered(existing, WINDOWS_SERVICE, sc.ServiceName))
                        continue;

                     var app = CreateApplication(WINDOWS_SERVICE, sc.DisplayName, sc.ServiceName, serverName);
                     app.Service_Status = sc.Status == ServiceControllerStatus.Running ? "Running" : "Stopped";

                     Register(app, existing);
                  }
                  catch (Exception ex)
                  {
                     LOGGER.Info($"DiscoverWindowsServices encountered an error on server {serverName} while registering {sc.ServiceName}: {ex.Message}");
                     LogWriter log = new LogWriter();
                     log.Error(ex);
                  }
               }
            }
         }
         catch (Exception ex)
         {
            LOGGER.Info($"DiscoverWindowsServices encountered an error on server {serverName}: {ex.Message}");
            LogWriter log = new LogWriter();
            log.Error(ex);
         }
      }

      private void DiscoverScheduledTasks(string serverName, List<ApplicationModel> existing)
      {
         try
         {
            LOGGER.Info($"Discovering scheduled tasks on server {serverName}");

            // Initialize TaskService targeting the specific remote server
            using (TaskService ts = new TaskService(serverName))
            {
               // recurse = true: tasks in every folder, not only the root
               foreach (WinTask t in ts.RootFolder.EnumerateTasks(task => Matches(task.Name) || Matches(task.Path), true))
               {
                  using (t)
                  {
                     try
                     {
                        // the health checker calls ts.GetTask(url_or_app_name): the name works for root tasks, subfolder tasks need the path
                        string taskName = t.Folder.Path == "\\" ? t.Name : t.Path;

                        if (IsRegistered(existing, SCHEDULED_TASK, taskName))
                           continue;

                        var app = CreateApplication(SCHEDULED_TASK, t.Name, taskName, serverName);
                        app.Is_Enabled = t.Enabled;

                        Register(app, existing);
                     }
                     catch (Exception ex)
                     {
                        LOGGER.Info($"DiscoverScheduledTasks encountered an error on server {serverName} while registering {t.Path}: {ex.Message}");
                        LogWriter log = new LogWriter();
                        log.Error(ex);
                     }
                  }
               }
            }
         }
         catch (Exception ex)
         {
            LOGGER.Info($"DiscoverScheduledTasks encountered an error on server {serverName}: {ex.Message}");
            LogWriter log = new LogWriter();
            log.Error(ex);
         }
      }

      private ApplicationModel CreateApplication(string applicationType, string applicationName, string urlOrAppName, string serverName)
      {
         var createdBy = ConfigurationManager.AppSettings["created_by"];

         return new ApplicationModel
         {
            Id = Guid.NewGuid().ToString(),
            Application_Name = applicationName,
            Description = $"Auto-discovered {applicationType} on {serverName}",
            Application_Type = applicationType,
            IP_Address = serverName,
            URL_Or_App_Name = urlOrAppName,
            Working_Status = (int)WorkingStatus.Working, // the health checker flips it to Not Working if it is failing
            Created_Date = DateTime.UtcNow,
            Created_By = !string.IsNullOrEmpty(createdBy) ? createdBy : "AutoDiscovery"
         };
      }

      // Inserts the application as standalone (no row in application_group_applications)
      private void Register(ApplicationModel app, List<ApplicationModel> existing)
      {
         _applicationRepository.Insert(app);
         existing.Add(app); // so the same component is not registered twice in one run
         _registeredCount++;

         LOGGER.Info($"Registered {app.Application_Type} {app.URL_Or_App_Name} on server {app.IP_Address}");
      }

      private List<string> GetTargetServers()
      {
         var targetServers = ConfigurationManager.AppSettings["target_servers"];
         if (string.IsNullOrWhiteSpace(targetServers))
            return new List<string>();

         return targetServers
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
      }

      private static bool Matches(string value)
      {
         return !string.IsNullOrEmpty(value) && value.IndexOf(MATCH_TEXT, StringComparison.OrdinalIgnoreCase) >= 0;
      }

      // already registered (deleted rows included) with the same type and service / task name
      private static bool IsRegistered(List<ApplicationModel> existing, string applicationType, string urlOrAppName)
      {
         var key = NormalizeKey(urlOrAppName);

         return existing.Any(a => string.Equals(a.Application_Type, applicationType, StringComparison.OrdinalIgnoreCase)
            && string.Equals(NormalizeKey(a.URL_Or_App_Name), key, StringComparison.OrdinalIgnoreCase));
      }

      // "\MyTask" and "MyTask" refer to the same root-folder task
      private static string NormalizeKey(string value)
      {
         return (value ?? string.Empty).Trim().TrimStart('\\');
      }
   }
}
