using ModelsLibrary.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ModelsLibrary.Models.API
{
	public class APIExceptionModel : Exception
	{
		public ApiResponseCode Code { get; set; }
	}
}