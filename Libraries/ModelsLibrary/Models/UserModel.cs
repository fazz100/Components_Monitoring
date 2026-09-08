using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary.Models
{
	public class UserModel
	{
		/*
		 create table [user] 
		(
		[user_id] bigint,
		username nvarchar(50),
		[password] nvarchar(max),
		salt nvarchar(max),
		first_name nvarchar(100),
		last_name nvarchar(100),
		created_date datetime,
		created_by bigint,
		updated_date datetime,
		updated_by bigint
		)
		 */
		public string Id { get; set; }

		
		public string Username { get; set; }
		public string PasswordString { get; set; }
		public byte[] Password { get; set; }
		public byte[] Salt { get; set; }
		public byte[] Key { get; set; }
		public byte[] IV { get; set; }
		public string First_Name { get; set; }
		public string FirstName
		{
			get => First_Name;
			set => First_Name = value;
		}
		public string Last_Name { get; set; }
		public string LastName
		{
			get => Last_Name;
			set => Last_Name = value;
		}
		public DateTime Created_Date { get; set; }
		public DateTime CreatedDate
		{
			get => Created_Date;
			set => Created_Date = value;
		}
		public string Created_By { get; set; }
		public string CreatedBy
		{
			get => Created_By;
			set => Created_By = value;
		}
		public DateTime Updated_Date { get; set; }
		public DateTime UpdatedDate
		{
			get => Updated_Date;
			set => Updated_Date = value;
		}
		public string Updated_By { get; set; }
		public string UpdatedBy
		{
			get => Updated_By;
			set => Updated_By = value;
		}
		public bool Is_Deleted { get; set; }
		public bool IsDeleted
		{
			get => Is_Deleted;
			set => Is_Deleted = value;
		}


	}
}
