using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
	public interface IApplicationExceptionRepository
	{
		ApplicationExceptionsModel Get(string id);
		List<ApplicationExceptionsModel> GetAll(string appName = null);
		void Insert(ApplicationExceptionsModel model);
		void Delete(string id);
	}
}
