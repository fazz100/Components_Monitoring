using ComponentsMonitoringAPI.Attributes;
using ComponentsMonitoringAPI.BLL.Interfaces;
using ComponentsMonitoringAPI.BLL.Services;
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
	public class UserController : CustomApiController
	{
		IUserService _userService;
		public UserController()
		{
			_userService = new UserService();

		}

		[HttpPost]
		[Route("api/user/create")]
		public IHttpActionResult CreateUser([FromBody] UserModel model)
		{

			return TryCatchWrapper<bool>(() =>
			{
				model.Id = Guid.NewGuid().ToString();
				model.Created_Date = DateTime.UtcNow;
				string userId = CurrentUserId;
				model.CreatedBy = userId;
				_userService.CreateUser(model);
				return true;
			});
		}

		[HttpPost]
		[Route("api/user/login")]
		public IHttpActionResult Login([FromBody] UserModel model)
		{

			return TryCatchWrapper<LoginResponseModel>(() =>
			{

				var response = _userService.Login(model);
				return response;
			});
		}

		[HttpPost]
		[Route("api/auth/logout")]
		[TokenAuthorize]
		public IHttpActionResult Logout()
		{

			return TryCatchWrapper<bool>(() =>
			{

				var token = Request.Headers.Authorization?.Parameter;

				var response = _userService.Logout(token);
				
				return response;
			});
			
		}

		[HttpGet]
		[Route("api/user/get-user/{searchTerm?}")]
		[TokenAuthorize]
		public IHttpActionResult GetUser(string searchTerm = null)
		{

			return TryCatchWrapper<List<UserModel>>(() =>
			{
				var userList = _userService.GetUsers(searchTerm);
				return userList;
			});
		}

		[HttpPost]
		[Route("api/user/update")]
		[TokenAuthorize]
		public IHttpActionResult UpdateUser([FromBody] UserModel model)
		{

			return TryCatchWrapper<bool>(() =>
			{
				string userId = CurrentUserId;
				model.Updated_By = userId;
				model.UpdatedDate = DateTime.UtcNow;
				_userService.UpdateUser(model);
				return true;
			});
		}

		[HttpPost]
		[Route("api/user/change-password")]
		public IHttpActionResult ChangePassword([FromBody] UserModel model)
		{

			return TryCatchWrapper<bool>(() =>
			{
				string userId = CurrentUserId;
				model.Updated_By = userId;
				_userService.ChangePassword(model);
				return true;
			});
		}
	}
}
