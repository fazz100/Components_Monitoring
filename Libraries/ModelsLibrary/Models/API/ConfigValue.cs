using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary.Models.API
{
	public class ConfigValue
	{
		public static string MSMQ_SOURCE = ConfigurationManager.AppSettings["automated_uploads_processing_message_queuer"];
		public static string JOB_NUMBER = ConfigurationManager.AppSettings["test_job_number"];
		public static string SUB_JOB_NUMBER = ConfigurationManager.AppSettings["test_sub_job_number"];

		public static string SFTP_HOST = ConfigurationManager.AppSettings["sftp_host"];
		public static string SFTP_PORT = ConfigurationManager.AppSettings["sftp_port"];
		public static string SFTP_USERNAME = ConfigurationManager.AppSettings["sftp_username"];
		public static string SFTP_PASSWORD = ConfigurationManager.AppSettings["sftp_password"];
		public static string SFTP_DESTINATION = ConfigurationManager.AppSettings["sftp_remote_directory_destination_path"];
	}
}
