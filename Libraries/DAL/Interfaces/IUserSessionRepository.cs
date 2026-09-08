using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
	public interface IUserSessionRepository
	{
		void Insert(UserSessionModel session);

		UserSessionModel GetByToken(string token);
		void LogoutSession(UserSessionModel model);
		void ExtendSession(UserSessionModel model);
	}
}
