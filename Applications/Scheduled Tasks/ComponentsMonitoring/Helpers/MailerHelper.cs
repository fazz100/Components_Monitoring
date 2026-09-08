using Dg3.CommonLibraries;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ComponentsMonitoring.Helpers
{
	public class MailerHelper
	{
      public static void Send(MailMessage mailMessage)
      {
         try
         {
            mailMessage.IsBodyHtml = true;

            string mailHost = ConfigurationManager.AppSettings["smtp_host"];
            //mailHost = "localhost";

            int mailPort = 25;
            int.TryParse(ConfigurationManager.AppSettings["smtp_port"], out mailPort);

            using (SmtpClient client = new SmtpClient(mailHost, mailPort))
            {
               client.EnableSsl = false;

               string smtpUsername = ConfigurationManager.AppSettings["smtp_username"];
               string smtpPassword = ConfigurationManager.AppSettings["smtp_password"];
               client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

               client.Send(mailMessage);
            }
         }
         catch (Exception ex)
         {
            LogWriter log = new LogWriter();
            log.Error(ex);
         }
      }

      public static string FormatEmailAddresses(string emailAddresses)
      {
         var delimiters = new[] { ',', ';' };

         var addresses = emailAddresses.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

         return string.Join(",", addresses);
      }
   }
}
