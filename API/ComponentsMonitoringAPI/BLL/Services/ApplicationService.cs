using ComponentsMonitoringAPI.BLL.Interfaces;
using DAL.Interfaces;
using Dg3.CommonLibraries;
using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ComponentsMonitoringAPI.BLL.Services
{
   public class ApplicationService : IApplicationService
   {
      private readonly IApplicationRepository _repo;
      private readonly IApplicationDatabaseRepository _appDBRepo;
      private static LogWriter LOGGER = new LogWriter();

      public ApplicationService() { 
         _repo = new DAL.Repositories.ApplicationRepository();
         _appDBRepo = new DAL.Repositories.ApplicationDatabaseRepository(); 
      }

      public List<ApplicationModel> GetApplications(string type,string appName=null, bool includeExceptions = true, bool includeInactiveGroups = false)
      {
         var list = _repo.GetAll(type, appName, includeExceptions, includeInactiveGroups);

         foreach (var l in list)
         {
            l.Databases = _appDBRepo.GetByAppId(l.Id);
         }

         return list;
      }

      public ApplicationModel GetById(string id)
      {
         var model=_repo.Get(id);
         model.Databases = _appDBRepo.GetByAppId(model.Id);
         return model;
      }

      public void Create(ApplicationModel model)
      { 
         _repo.Insert(model);

         foreach (var appDb in model.Databases)
         {
				try
				{
               appDb.Id= Guid.NewGuid().ToString();
               appDb.Created_By = model.Created_By;
               appDb.Created_Date = DateTime.UtcNow;
               appDb.Application_Id = model.Id;
               _appDBRepo.Insert(appDb);
            }
				catch (Exception ex)
				{
               LOGGER.Error("An error has occurred while saving database " + appDb.App_Database_Name + ": " + ex.Message + " " + ex.InnerException.Message);

               throw;
				}
            
         }
      
      }
      public void Update(ApplicationModel model) => _repo.Update(model);
      public void Delete(string id, string user) => _repo.Delete(id, user);
   }
}