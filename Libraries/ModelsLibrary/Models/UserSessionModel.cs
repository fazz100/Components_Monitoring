using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary.Models
{
	public class UserSessionModel
	{
      public string Id { get; set; }
      public string User_Id { get; set; }
      public string Token { get; set; }
      public DateTime Issued_At { get; set; }
      public DateTime Expires_At { get; set; }
      public bool Is_Revoked { get; set; }
   }
}
