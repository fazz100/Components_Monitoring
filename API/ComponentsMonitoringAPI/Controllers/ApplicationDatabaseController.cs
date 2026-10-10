using System;
using System.Web.Http;
using ModelsLibrary.Models;
using ComponentsMonitoringAPI.BLL.Interfaces;
using ComponentsMonitoringAPI.BLL.Services;

namespace ComponentsMonitoringAPI.Controllers
{
   [RoutePrefix("api/application-databases")]
   [Authorize]
   public class ApplicationDatabaseController : CustomApiController
   {
      private readonly IApplicationDatabaseService _service;
      public ApplicationDatabaseController() { _service = new ApplicationDatabaseService(); }

      [HttpPost]
      [Route("save")]
      public IHttpActionResult Save(ApplicationDatabaseModel model) => TryCatchWrapper(() =>
      {
         string userId = CurrentUserId;

         model.Id = Guid.NewGuid().ToString();
         model.Created_By = userId;
         model.Created_Date = DateTime.UtcNow;
         bool result = _service.SaveDatabase(model);
         return model.Id;
      });

      [HttpPost]
      [Route("update")]
      public IHttpActionResult Update(ApplicationDatabaseModel model) => TryCatchWrapper(() =>
      {
         bool result = _service.UpdateDatabase(model);
         return result;
      });

      [HttpDelete]
      [Route("delete/{id}")]
      public IHttpActionResult Delete(string id) => TryCatchWrapper(() =>
      {

         bool result = _service.DeleteDatabase(id);
         return result;
      });

      [HttpPost]
      [Route("test-connection")]
      public IHttpActionResult TestConnection([FromBody] string connectionString) => TryCatchWrapper(() =>
      {
         return _service.TestDatabaseConnection(connectionString);
      });
   }
}