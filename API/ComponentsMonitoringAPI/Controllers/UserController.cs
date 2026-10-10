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
	[Authorize]
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

		// Login is handled by the OAuth /token endpoint (see Providers/ApplicationOAuthProvider.cs)

		[HttpPost]
		[Route("api/auth/logout")]
		public IHttpActionResult Logout([FromBody] LogoutRequestModel model)
		{

			return TryCatchWrapper<bool>(() =>
			{
				// revokes the refresh token; the short-lived access token simply expires
				return _userService.Logout(model?.RefreshToken, CurrentUserId);
			});

		}

		[HttpGet]
		[Route("api/user/get-user/{searchTerm?}")]
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
