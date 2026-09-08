using ModelsLibrary.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
	public interface IResponseCodeRepository
	{
		List<ReponseCodeModel> GetAll();
		ReponseCodeModel GetByCode(int code);
	}
}
