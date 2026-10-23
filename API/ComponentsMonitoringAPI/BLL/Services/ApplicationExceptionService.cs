using ComponentsMonitoringAPI.BLL.Interfaces;
using DAL.Interfaces;
using DAL.Repositories;
using ModelsLibrary.Models;
using ModelsLibrary.Models.API;
using ModelsLibrary.Models.Enums;
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
         if (string.IsNullOrEmpty(model.Application_Id)) model.Application_Id = null;
         if (string.IsNullOrEmpty(model.Application_Group_Id)) model.Application_Group_Id = null;

         // an exception silences either one application or a whole application group, never both or neither
         if ((model.Application_Id == null) == (model.Application_Group_Id == null))
         {
            var apiException = new APIExceptionModel();
            apiException.Code = ApiResponseCode.ExceptionTargetRequired;
            throw apiException;
         }

         _repository.Insert(model);
         return true;
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