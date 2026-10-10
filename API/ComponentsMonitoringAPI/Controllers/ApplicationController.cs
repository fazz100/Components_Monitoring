using ComponentsMonitoringAPI.BLL.Interfaces;
using ComponentsMonitoringAPI.BLL.Services;
using ModelsLibrary.Models;
using ModelsLibrary.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ComponentsMonitoringAPI.Controllers
{
   [Authorize]
   public class ApplicationController : CustomApiController
    {
      private readonly IApplicationService _service;
      public ApplicationController() { _service = new ApplicationService(); }

      [HttpGet]
      [Route("api/applications/{type?}/{appName?}/{includeExceptions?}")]
      public IHttpActionResult GetList(string type=null, string appName = null, bool includeExceptions = true) => TryCatchWrapper(() => _service.GetApplications(type, appName, includeExceptions));


      [HttpPost]
      [Route("api/applications/create")]
      public IHttpActionResult Create(ApplicationModel model) => TryCatchWrapper(() => {
         string userId = CurrentUserId;

         model.Id = Guid.NewGuid().ToString(); 
         model.Created_Date = DateTime.UtcNow;
         //model.Working_Status = (int)WorkingStatus.Working;          
         model.Created_By = userId;
         // Cleanup
         if (model.Application_Type != "scheduled task") model.Is_Enabled = null;
         if (model.Application_Type != "windows service") model.Service_Status = null;

         _service.Create(model);
         return true;
      });

      [HttpPut]
      [Route("api/applications/update")]
      public IHttpActionResult Update(ApplicationModel model) => TryCatchWrapper(() => {
         string userId = CurrentUserId;

         model.Updated_Date = DateTime.UtcNow; 
         model.Updated_By = userId;
         // Cleanup
         if (model.Application_Type != "scheduled task") model.Is_Enabled = null;
         if (model.Application_Type != "windows service") model.Service_Status = null;

         _service.Update(model);
         return true;
      });

      [HttpDelete]
      [Route("api/applications/delete/{id}")]
      public IHttpActionResult Delete(string id) => TryCatchWrapper(() => {
         string userId = CurrentUserId;
         _service.Delete(id, userId);
         return true;
      });
   }
}
