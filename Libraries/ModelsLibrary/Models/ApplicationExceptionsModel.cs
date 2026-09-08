using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary.Models
{
	public class ApplicationExceptionsModel
	{

		public string Id { get; set; }
		public string Application_Id { get; set; }
		public string Application_Name { get; set; }

		public string Reason_For_Exception { get; set; }
		public string Created_By { get; set; }
		public DateTime Created_Date { get; set; }

	}
}
