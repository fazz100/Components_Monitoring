using ComponentsMonitoringAPI.BLL.Interfaces;
using DAL.Interfaces;
using DAL.Repositories;
using ModelsLibrary.Models;
using System;
using System.Collections.Generic;

namespace ComponentsMonitoringAPI.BLL.Services
{
   public class ApplicationExceptionService : IApplicationExceptionService
   {
      private readonly IApplicationExceptionRepository _repository;

      public ApplicationExceptionService()
      {
         _repository = new ApplicationExceptionRepository();
      }

      public ApplicationExceptionsModel GetById(string id)
      {
         try
         {
            if (string.IsNullOrEmpty(id)) return null;
            return _repository.Get(id);
         }
         catch (Exception)
         {
            throw;
         }
      }

      public List<ApplicationExceptionsModel> GetAllExceptions(string appName = null)
      {
         try
         {
            return _repository.GetAll(appName);
         }
         catch (Exception)
         {
            throw;
         }
      }

      public bool AddException(ApplicationExceptionsModel model)
      {
         try
         {
            _repository.Insert(model);
            return true;
         }
         catch (Exception)
         {
            throw;
         }
      }

      public bool RemoveException(string id)
      {
         try
         {
            if (string.IsNullOrEmpty(id)) return false;

            _repository.Delete(id);
            return true;
         }
         catch (Exception)
         {
            throw;
         }
      }
   }
}