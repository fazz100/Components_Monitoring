using ComponentsMonitoringAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ComponentsMonitoringAPI
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{

			
			string reactAppUrl = WebConfigurationManager.AppSettings["reactAppUrl"];
			// Web API configuration and services
			// Enable CORS globally
			var cors = new EnableCorsAttribute(
				reactAppUrl,           // allowed origin
				"Content-Type,Accept,X-Api-Token,Authorization", // allowed headers
				"GET,POST,PUT,DELETE,OPTIONS"      // allowed methods
				); // Origin, Headers, Methods
			cors.SupportsCredentials = true;  // <— Add this line
			config.EnableCors(cors);



			// Token handler
			config.MessageHandlers.Add(new TokenValidationHandler());
			


			// Web API routes
			config.MapHttpAttributeRoutes();


			config.Routes.MapHttpRoute(
				 name: "DefaultApi",
				 routeTemplate: "api/{controller}/{id}",
				 defaults: new { id = RouteParameter.Optional }
			);

		}
	}
}
