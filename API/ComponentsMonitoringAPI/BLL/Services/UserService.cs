using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using ComponentsMonitoringAPI.BLL.Interfaces;
using DAL.Interfaces;
using DAL.Repositories;
using Dg3.CommonLibraries;
using ModelsLibrary.Helpers;
using ModelsLibrary.Models;
using ModelsLibrary.Models.API;
using ModelsLibrary.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using ComponentsMonitoringAPI.Helpers;
using System.Collections;

namespace ComponentsMonitoringAPI.BLL.Services
{
	public class UserService : IUserService
	{
		IUserRepository _userRepository;
		IRefreshTokenRepository _refreshTokenRepository;
		private static LogWriter LOGGER = new LogWriter();
		public UserService()
		{
			_userRepository = new UserRepository();
			_refreshTokenRepository = new RefreshTokenRepository();
		}

		public void CreateUser(UserModel model)
		{
			try
			{
				var username = model.Username;
				var password = model.PasswordString;

				//check if username already exists
				UserModel savedUserModel = _userRepository.GetByUsername(username);

				if (savedUserModel == null)
				{
					
					model.Password = GetPasswordByte(password);

					model.CreatedDate = DateTime.UtcNow;
					model.IsDeleted = false;

					_userRepository.Insert(model);
				}
				else
				{
					var apiException = new APIExceptionModel();
					apiException.Code = ApiResponseCode.UserExists;
					throw apiException;
				}

			}
			catch (Exception)
			{
				throw;
			}
		}

		public bool Logout(string refreshToken, string userId)
		{
			// only the owner of the refresh token can revoke it
			if (!string.IsNullOrEmpty(refreshToken) && !string.IsNullOrEmpty(userId))
			{
				_refreshTokenRepository.RevokeByHash(HashHelper.ComputeSha512Hash(refreshToken), userId);
			}

			return true;
		}

		public List<UserModel> GetUsers(string searchTerm = null)
		{
			try
			{
				LOGGER.Info("Fetching all user List ");
				return _userRepository.GetAll(searchTerm);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public void UpdateUser(UserModel model)
		{
			try
			{
				var user = _userRepository.GetById(model.Id);

				user.FirstName = string.IsNullOrEmpty(model.FirstName) ? user.FirstName : model.FirstName;
				user.LastName = string.IsNullOrEmpty(model.LastName) ? user.LastName : model.LastName;
				user.IsDeleted = model.IsDeleted;
				user.Updated_Date = DateTime.UtcNow;
				_userRepository.Update(user);

			}
			catch (Exception)
			{
				throw;
			}
		}


		public void ChangePassword(UserModel model)
		{
			try
			{
				var password = model.PasswordString;

				UserModel savedUserModel = _userRepository.GetById(model.Id);

				model.Password = GetPasswordByte(password);


				model.FirstName = savedUserModel.FirstName;
				model.LastName = savedUserModel.LastName;
				model.IsDeleted = savedUserModel.IsDeleted;
				model.Updated_Date = DateTime.UtcNow;

				_userRepository.Update(model);

			}
			catch (Exception ex)
			{
				throw;
			}
		}

		private byte[] GetPasswordByte(string password)
		{
			var encryptedPepper = WebConfigurationManager.AppSettings["encrypted_pepper"];
			var pepper = DpapiHelper.Decrypt(encryptedPepper);

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
		public bool VerifyPassword(string inputPassword, string storedRecord)
		{
			var encryptedPepper = WebConfigurationManager.AppSettings["encrypted_pepper"];
			var pepper = DpapiHelper.Decrypt(encryptedPepper);

			var parts = storedRecord.Split(':');
			byte[] salt = Convert.FromBase64String(parts[0]);
			byte[] savedHash = Convert.FromBase64String(parts[1]);

			var argon2 = new Argon2id(Encoding.UTF8.GetBytes(inputPassword + pepper));
			argon2.Salt = salt;
			argon2.DegreeOfParallelism = 8;
			argon2.Iterations = 4;
			argon2.MemorySize = 65536;

			byte[] newHash = argon2.GetBytes(32);

			// Use a bitwise comparison to prevent timing attacks
			return StructuralComparisons.StructuralEqualityComparer.Equals(savedHash, newHash);
		}
	}
}