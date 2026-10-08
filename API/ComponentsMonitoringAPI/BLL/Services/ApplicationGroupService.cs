using ComponentsMonitoringAPI.BLL.Interfaces;
using DAL.Interfaces;
using DAL.Repositories;
using ModelsLibrary.Models;
using ModelsLibrary.Models.API;
using ModelsLibrary.Models.Enums;
using System.Collections.Generic;

namespace ComponentsMonitoringAPI.BLL.Services
{
   public class ApplicationGroupService : IApplicationGroupService
   {
      private readonly IApplicationGroupRepository _repository;

      public ApplicationGroupService()
      {
         _repository = new ApplicationGroupRepository();
      }

      public List<ApplicationGroupModel> GetAll(string searchTerm = null) => _repository.GetAll(searchTerm);

      public ApplicationGroupModel GetById(string id)
      {
         if (string.IsNullOrEmpty(id)) return null;
         return _repository.Get(id);
      }

      public void Create(ApplicationGroupModel model)
      {
         EnsureNameIsUnique(model);
         _repository.Insert(model);
      }

      public void Update(ApplicationGroupModel model)
      {
         EnsureNameIsUnique(model);
         _repository.Update(model);
      }

      public void Delete(string id, string user) => _repository.Delete(id, user);

      private void EnsureNameIsUnique(ApplicationGroupModel model)
      {
         var existing = _repository.GetByName(model.Application_Group_Name);

         if (existing != null && existing.Id != model.Id)
         {
            var apiException = new APIExceptionModel();
            apiException.Code = ApiResponseCode.ApplicationGroupExists;
            throw apiException;
         }
      }
   }
}
