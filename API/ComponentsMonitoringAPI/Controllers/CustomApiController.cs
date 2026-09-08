using ComponentsMonitoringAPI.BLL.Interfaces;
using ComponentsMonitoringAPI.BLL.Services;
using Dg3.CommonLibraries;
using ModelsLibrary.Models;
using ModelsLibrary.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ComponentsMonitoringAPI.Controllers
{
    public class CustomApiController : ApiController
    {
		IResponseCodeService _responseCode;
		private static LogWriter LOGGER = new LogWriter();
		public CustomApiController()
		{
			_responseCode = new ResponseCodeService();
		}

		protected string CurrentUserId
		{
			get
			{
				if (Request.Properties.ContainsKey("CurrentUserId"))
				{
					return Request.Properties["CurrentUserId"] as string;
				}
				return null;
			}
		}

		protected UserSessionModel CurrentSession
		{
			get
			{
				if (Request.Properties.ContainsKey("UserSession"))
				{
					return Request.Properties["UserSession"] as UserSessionModel;
				}
				return null;
			}
		}


		protected IHttpActionResult TryCatchWrapper<T>(Func<T> action)
		{
			try
			{
				var returnData = action();

				var response = new APIResponseModel<T>();
				response.Code = 200;
				response.Message = "Success";
				response.Data = returnData;
				response.Details = null;

				return Ok(response);
			}
			catch (APIExceptionModel ex)
			{
				LOGGER.Info(ex);
				var responseCode=_responseCode.GetResponseCode((int)ex.Code);
				var response = new APIResponseModel<T>();
				response.Code = responseCode.Code;
				response.Message = responseCode.Message;
				response.Data = default;
				response.Details = ex.Message;

				return Content(HttpStatusCode.InternalServerError, response);
			}
			catch (Exception ex)
			{
				LOGGER.Error(ex);
				var response = new APIResponseModel<T>();
				response.Code = 500;
				response.Message = "Error";
				response.Data = default;
				response.Details = ex;

				return Content(HttpStatusCode.InternalServerError, response);
			}
		}
	}
}
