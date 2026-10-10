using ModelsLibrary.Models;

namespace DAL.Interfaces
{
   public interface IApiClientRepository
   {
      ApiClientModel GetActiveByClientId(string clientId);
   }
}
