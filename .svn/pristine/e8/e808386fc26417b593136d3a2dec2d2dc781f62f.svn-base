using DAL.Interfaces;
using DAL.Repositories;
using Konscious.Security.Cryptography;
using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CreateAdminUser
{
	class Program
	{
		static void Main(string[] args)
		{

			Console.WriteLine("Creating admin user start...");
			
			Console.WriteLine(CreateUser());
			Console.Write("Press any key to continue...");
			Console.ReadKey();
		}

		public static string CreateUser()
		{
			IUserRepository _userRepository = new UserRepository();
			try
			{
				var username = ConfigurationManager.AppSettings["username"];
				var password = ConfigurationManager.AppSettings["password"];
				var firstname = ConfigurationManager.AppSettings["firstname"];
				var lastname = ConfigurationManager.AppSettings["lastname"];

				//check if username already exists
				UserModel savedUserModel = _userRepository.GetByUsername(username);

				if (savedUserModel == null)
				{
					var model = new UserModel();
					model.Id = Guid.NewGuid().ToString();
					model.Username = username;
					model.Password = GetPasswordByte(password);
					model.FirstName = firstname;
					model.LastName = lastname;
					model.CreatedDate = DateTime.UtcNow;
					model.CreatedBy = model.Id;
					model.IsDeleted = false;

					_userRepository.Insert(model);
					return "Successfully created user!";
				}
				else
				{
					/*
					var apiException = new APIExceptionModel();
					apiException.Code = ApiResponseCode.UserExists;
					throw apiException;
					*/
					return "User already exists!";
				}

			}
			catch (Exception)
			{
				throw;
			}
		}

		private static byte[] GetPasswordByte(string password)
		{
			var encryptedPepper = ConfigurationManager.AppSettings["encrypted_pepper"];
			//LOGGER.Info("encryptedPepper " + encryptedPepper);
			var pepper = DpapiHelper.Decrypt(encryptedPepper);
			//LOGGER.Info("pepper " + encryptedPepper);

			// 1. Generate a random salt (Argon2 needs this, but you don't encrypt it)
			byte[] salt = new byte[16];
			using (var rng = new RNGCryptoServiceProvider()) { rng.GetBytes(salt); }

			// 2. Setup Argon2id
			var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password + pepper));
			argon2.Salt = salt;
			argon2.DegreeOfParallelism = 8; // Number of threads
			argon2.Iterations = 4;          // Number of passes
			argon2.MemorySize = 65536;      // 64 MB RAM usage

			// 3. Get the hash
			byte[] hash = argon2.GetBytes(32);

			// 4. Store as a single string (Salt + Hash combined)
			// format: [salt_base64]:[hash_base64]
			var passwordHash = Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
			byte[] passwordByteArray = Encoding.UTF8.GetBytes(passwordHash);

			return passwordByteArray;
		}
	}
}
