using ComponentsMonitoringAPI.Attributes;
using ComponentsMonitoringAPI.BLL.Interfaces;
using ComponentsMonitoringAPI.BLL.Services;
using ModelsLibrary.Models;
using System;
using System.Web.Http;

namespace ComponentsMonitoringAPI.Controllers
{
   [RoutePrefix("api/application-groups")]
   [TokenAuthorize]
   public class ApplicationGroupController : CustomApiController
   {
      private readonly IApplicationGroupService _service;

      public ApplicationGroupController()
      {
         _service = new ApplicationGroupService();
      }

      [HttpGet]
      [Route("all")]
      public IHttpActionResult GetAll(string searchTerm = null) => TryCatchWrapper(() => _service.GetAll(searchTerm));

      [HttpGet]
      [Route("{id}")]
      public IHttpActionResult Get(string id) => TryCatchWrapper(() => _service.GetById(id));

      [HttpPost]
      [Route("create")]
      public IHttpActionResult Create(ApplicationGroupModel model) => TryCatchWrapper(() =>
      {
         model.Id = Guid.NewGuid().ToString();
         model.Created_Date = DateTime.UtcNow;
         model.Created_By = CurrentUserId;

         _service.Create(model);
         return model.Id;
      });

      [HttpPost]
      [Route("update")]
      public IHttpActionResult Update(ApplicationGroupModel model) => TryCatchWrapper(() =>
      {
         model.Updated_Date = DateTime.UtcNow;
         model.Updated_By = CurrentUserId;

         _service.Update(model);
         return true;
      });

      [HttpPost]
      [Route("delete/{id}")]
      public IHttpActionResult Delete(string id) => TryCatchWrapper(() =>
      {
         _service.Delete(id, CurrentUserId);
         return true;
      });
   }
}
