using System;

namespace ModelsLibrary.Models
{
   public class ApiClientModel
   {
      public string Id { get; set; }
      public string Client_Name { get; set; }
      public string Client_Id { get; set; }
      public string Client_Secret_Hash { get; set; } // null = public client (e.g. the React SPA)
      public bool Is_Active { get; set; }
      public DateTime Created_Date { get; set; }
   }
}
