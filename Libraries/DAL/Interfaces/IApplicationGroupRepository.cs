using ModelsLibrary.Models;
using System.Collections.Generic;

namespace DAL.Interfaces
{
   public interface IApplicationGroupRepository
   {
      ApplicationGroupModel Get(string id);
      List<ApplicationGroupModel> GetAll(string searchTerm = null);
      ApplicationGroupModel GetByName(string name);
      void Insert(ApplicationGroupModel model);
      void Update(ApplicationGroupModel model);
      void Delete(string id, string updatedBy);
   }
}
