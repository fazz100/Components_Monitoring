using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
	public interface IUserRepository
	{
		List<UserModel> GetAll(string searchTerm = null);
		void Insert(UserModel model);

		UserModel GetById(string id);
		UserModel GetByUsername(string username);
		void Update(UserModel model);



	}
}
