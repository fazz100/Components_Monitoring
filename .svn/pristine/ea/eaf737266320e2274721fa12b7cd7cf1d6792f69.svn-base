using ComponentsMonitoringAPI.Attributes;
using ComponentsMonitoringAPI.BLL.Interfaces;
using ComponentsMonitoringAPI.BLL.Services;
using ModelsLibrary.Models;
using System;
using System.Web.Http;

namespace ComponentsMonitoringAPI.Controllers
{
   [TokenAuthorize]
   public class ApplicationExceptionController : CustomApiController
   {
      private readonly IApplicationExceptionService _service;

      public ApplicationExceptionController()
      {
         _service = new ApplicationExceptionService();
      }

      [HttpGet]
      [Route("api/exceptions/all/{appName?}")]
      public IHttpActionResult GetAll(string appName = null) => TryCatchWrapper(() => _service.GetAllExceptions(appName));

      [HttpGet]
      [Route("api/exceptions/{id}")]
      public IHttpActionResult Get(string id) => TryCatchWrapper(() => _service.GetById(id));

      [HttpPost]
      [Route("api/exceptions/create")]
      public IHttpActionResult Create(ApplicationExceptionsModel model) => TryCatchWrapper(() =>
      {
         string userId = CurrentUserId;

         model.Id = Guid.NewGuid().ToString();
         model.Created_Date = DateTime.UtcNow;

         if (string.IsNullOrEmpty(model.Created_By))
            model.Created_By = userId;

         _service.AddException(model);
         return true;
      });

      [HttpDelete]
      [Route("api/exceptions/delete/{id}")]
      public IHttpActionResult Delete(string id) => TryCatchWrapper(() =>
      {
         _service.RemoveException(id);
         return true;
      });
   }
}