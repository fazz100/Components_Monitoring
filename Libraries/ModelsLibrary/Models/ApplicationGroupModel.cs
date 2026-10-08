using System;

namespace ModelsLibrary.Models
{
   public class ApplicationGroupModel
   {
      public string Id { get; set; }
      public string Application_Group_Name { get; set; }
      public string Description { get; set; }
      public string Created_By { get; set; }
      public DateTime Created_Date { get; set; }
      public string Updated_By { get; set; }
      public DateTime? Updated_Date { get; set; }
      public bool Is_Deleted { get; set; }
   }
}
