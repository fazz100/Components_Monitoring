using ComponentsMonitoringAPI.BLL.Interfaces;
using DAL.Interfaces;
using DAL.Repositories;
using ModelsLibrary.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ComponentsMonitoringAPI.BLL.Services
{
	public class ResponseCodeService : IResponseCodeService
	{
		IResponseCodeRepository _responseCodeRepository;
		public ResponseCodeService()
		{
			_responseCodeRepository = new ResponseCodeRepository();
		}
		public ReponseCodeModel GetResponseCode(int code)
		{
			try
			{
				return _responseCodeRepository.GetByCode(code);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}