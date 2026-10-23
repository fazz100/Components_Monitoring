using Dg3.CommonLibraries;
using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace ComponentsMonitoringAlertDispatcher.Helpers
{
   public class MailerHelper
   {
      // Returns false if the email could not be sent, so the caller can keep the alerts pending
      public static bool Send(MailMessage mailMessage)
      {
         try
         {
            mailMessage.IsBodyHtml = true;

            string mailHost = ConfigurationManager.AppSettings["smtp_host"];

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

            return true;
         }
         catch (Exception ex)
         {
            LogWriter log = new LogWriter();
            log.Error(ex);
            return false;
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
