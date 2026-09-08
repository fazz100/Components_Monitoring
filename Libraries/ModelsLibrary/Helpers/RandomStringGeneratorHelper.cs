using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace ModelsLibrary.Helpers
{
	public class RandomStringGeneratorHelper
	{
      private static readonly char[] SafeChars =
          "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_@#$%".ToCharArray();

      public static string GenerateSafeRandomString(int minLength, int maxLength)
      {
         if (minLength < 1 || maxLength < minLength)
            throw new ArgumentException("Invalid min/max length values.");

         int length = RandomNumber(minLength, maxLength + 1); // upper bound is exclusive

         var result = new StringBuilder(length);

         byte[] buffer = new byte[sizeof(uint)];

         using (var rng = RandomNumberGenerator.Create())
         {
            for (int i = 0; i < length; i++)
            {
               rng.GetBytes(buffer);
               uint num = BitConverter.ToUInt32(buffer, 0);
               char selectedChar = SafeChars[num % SafeChars.Length];
               result.Append(selectedChar);
            }
         }

         return result.ToString();
      }

      // Helper to get a random number between min (inclusive) and max (exclusive)
      private static int RandomNumber(int min, int max)
      {
         byte[] buffer = new byte[4];
         using (var rng = RandomNumberGenerator.Create())
         {
            rng.GetBytes(buffer);
            int result = Math.Abs(BitConverter.ToInt32(buffer, 0));
            return result % (max - min) + min;
         }
      }


   }
}
