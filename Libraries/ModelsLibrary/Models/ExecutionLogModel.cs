using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary.Models
{
	public class ExecutionLogModel
	{
		public string Id { get; set; }
		public string Batch_Id { get; set; }
		public string Application_Id { get; set; }
		public string Server_Ip_Address { get; set; }
		public bool Working_Status { get; set; }
		public DateTime Date_Created { get; set; }
		public string Check_Status { get; set; } //success / failed
		public string Details { get; set; } //details, exception message
	}
}
