using ModelsLibrary.Models;
using System.Collections.Generic;

namespace ComponentsMonitoringAPI.BLL.Interfaces
{
   public interface IApplicationGroupService
   {
      List<ApplicationGroupModel> GetAll(string searchTerm = null);
      ApplicationGroupModel GetById(string id);
      void Create(ApplicationGroupModel model);
      void Update(ApplicationGroupModel model);
      void Delete(string id, string user);
   }
}
