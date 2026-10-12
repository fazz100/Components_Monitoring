using System;

namespace ModelsLibrary.Models
{
   public class RefreshTokenModel
   {
      public string Id { get; set; }
      public string User_Id { get; set; }
      public string Token_Hash { get; set; } // SHA-512 hex of the raw refresh token, the raw value is never stored
      public DateTime Issued_At { get; set; }
      public DateTime Expires_At { get; set; }
      public bool Is_Revoked { get; set; }
   }
}
