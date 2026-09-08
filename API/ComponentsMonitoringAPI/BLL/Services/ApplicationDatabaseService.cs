using ComponentsMonitoringAPI.BLL.Interfaces;
using DAL.Interfaces;
using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ComponentsMonitoringAPI.BLL.Services
{
	public class ApplicationDatabaseService: IApplicationDatabaseService
	{

      private readonly IApplicationDatabaseRepository _repo;
      public ApplicationDatabaseService() { _repo = new DAL.Repositories.ApplicationDatabaseRepository(); }

      public bool TestDatabaseConnection(string connectionString)
      {
         try
         {
            return _repo.TestDatabaseConnection(connectionString);
         }
         catch (Exception)
         {
            throw;
         }
      }

      public bool SaveDatabase(ApplicationDatabaseModel model)
      {
         try
         {
            _repo.Insert(model);
            return true;
         }
         catch (Exception)
         {
            throw;
         }

      }

      public bool UpdateDatabase(ApplicationDatabaseModel model)
      {
         try
         {
            _repo.Update(model);
            return true;
         }
         catch (Exception)
         {
            throw;
         }

      }

      public bool DeleteDatabase(string Id)
      {
         try
         {
            _repo.Delete(Id);
            return true;
         }
         catch (Exception)
         {
            throw;
         }
      }
   }
}