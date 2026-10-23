namespace ModelsLibrary.Models
{
   // Membership of an application in an application group (system); at most one row per application
   public class ApplicationGroupApplicationModel
   {
      public string Id { get; set; }
      public string Application_Group_Id { get; set; }
      public string Application_Id { get; set; }
   }
}
