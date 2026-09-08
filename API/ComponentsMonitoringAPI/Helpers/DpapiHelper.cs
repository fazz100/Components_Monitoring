using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ComponentsMonitoringAPI.Helpers
{
   public static class DpapiHelper
   {
      // Optional extra entropy (acts like an internal password for DPAPI)
      private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("3exWx7XF!@#gbaUkHnZmyt#!@V2i7SN!@#We9qBa");

      public static string Encrypt(string plainText)
      {
         byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

         // This line asks Windows to encrypt the data using the Local Machine credentials
         byte[] encryptedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.LocalMachine);

         return Convert.ToBase64String(encryptedBytes);
      }

      public static string Decrypt(string cipherText)
      {
         byte[] encryptedBytes = Convert.FromBase64String(cipherText);

         // This asks Windows to decrypt using the same Machine credentials
         byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, Entropy, DataProtectionScope.LocalMachine);

         return Encoding.UTF8.GetString(plainBytes);
      }
   }
}