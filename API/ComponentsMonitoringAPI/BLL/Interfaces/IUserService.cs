using ModelsLibrary.Models;
using ModelsLibrary.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentsMonitoringAPI.BLL.Interfaces
{
	public interface IUserService
	{
		void CreateUser(UserModel model);
		bool VerifyPassword(string inputPassword, string storedRecord);
		bool Logout(string refreshToken, string userId);
		List<UserModel> GetUsers(string searchTerm = null);

		void UpdateUser(UserModel model);
		void ChangePassword(UserModel model);
	}
}
