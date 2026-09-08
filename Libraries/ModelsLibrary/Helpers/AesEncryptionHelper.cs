using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary.Helpers
{
	public class AesEncryptionHelper
	{
      public class AesEncryptionResult
      {
         public string CipherTextBase64 { get; set; }
         public byte[] Key { get; set; }  // 32 bytes
         public byte[] IV { get; set; }   // 16 bytes
      }


      /// <summary>
      /// Encrypts a plain text using AES-256 with auto-generated key and IV
      /// </summary>
      public static AesEncryptionResult EncryptString(string plainText)
      {
         using (Aes aes = Aes.Create())
         {
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.GenerateKey(); // 32 bytes
            aes.GenerateIV();  // 16 bytes

            using (MemoryStream msEncrypt = new MemoryStream())
            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
               byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
               csEncrypt.Write(plainBytes, 0, plainBytes.Length);
               csEncrypt.FlushFinalBlock();

               return new AesEncryptionResult
               {
                  CipherTextBase64 = Convert.ToBase64String(msEncrypt.ToArray()),
                  Key = aes.Key,
                  IV = aes.IV
               };
            }
         }
      }

      /// <summary>
      /// Decrypts a base64 AES string using provided key and IV
      /// </summary>
      public static string DecryptString(string base64CipherText, byte[] key, byte[] iv)
      {
         if (key == null || key.Length != 32)
            throw new ArgumentException("Key must be 32 bytes (256 bits)");
         if (iv == null || iv.Length != 16)
            throw new ArgumentException("IV must be 16 bytes (128 bits)");

         using (Aes aes = Aes.Create())
         {
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] cipherBytes = Convert.FromBase64String(base64CipherText);

            using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
            using (ICryptoTransform decryptor = aes.CreateDecryptor())
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (StreamReader srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8))
            {
               return srDecrypt.ReadToEnd();
            }
         }
      }
   }
}
