using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary.Models.API
{
	public class LoginResponseModel
	{
      public bool IsAuthenticated { get; set; }
      public string Token { get; set; }
      public DateTime? ExpiresAt { get; set; }
      public string UserId { get; set; }
      public string Username { get; set; }
      public string FullName { get; set; }
      public string Message { get; set; }
   }
}
