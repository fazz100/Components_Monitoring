using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace ModelsLibrary.Helpers
{
   

   public static class HashHelper
   {
      public static string ComputeSha512Hash(string input)
      {
         using (SHA512 sha512 = SHA512.Create())
         {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha512.ComputeHash(inputBytes);

            // Convert hash bytes to hex string
            StringBuilder sb = new StringBuilder(hashBytes.Length * 2);
            foreach (byte b in hashBytes)
               sb.AppendFormat("{0:x2}", b);

            return sb.ToString();
         }
      }
   }
}
