using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ModelsLibrary.Models.Enums
{
	public enum ApiResponseCode
	{
		Success = 200,
		UserExists = 901,
		ApplicationGroupExists = 902,
		InternalServerError = 999,
	}
}