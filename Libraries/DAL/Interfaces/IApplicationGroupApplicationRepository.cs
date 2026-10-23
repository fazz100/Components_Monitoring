using ModelsLibrary.Models;

namespace DAL.Interfaces
{
   public interface IApplicationGroupApplicationRepository
   {
      ApplicationGroupApplicationModel GetByApplicationId(string applicationId);
      void Insert(ApplicationGroupApplicationModel model);
      void Update(ApplicationGroupApplicationModel model);
      void DeleteByApplicationId(string applicationId);
   }
}
