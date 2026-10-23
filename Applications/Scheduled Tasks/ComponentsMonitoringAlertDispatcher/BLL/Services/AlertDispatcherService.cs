using ComponentsMonitoringAlertDispatcher.BLL.Interfaces;
using ComponentsMonitoringAlertDispatcher.Helpers;
using DAL.Interfaces;
using DAL.Repositories;
using Dg3.CommonLibraries;
using ModelsLibrary.Helpers;
using ModelsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ComponentsMonitoringAlertDispatcher.BLL.Services
{
   // Sends one digest email per application group for failures flagged (alert_pending) by the health checker
   public class AlertDispatcherService : IAlertDispatcherService
   {
      private const string INDIVIDUAL_APPLICATIONS = "Individual Applications";

      private readonly IApplicationRepository _applicationRepository;
      private static LogWriter LOGGER = new LogWriter();

      public AlertDispatcherService()
      {
         _applicationRepository = new ApplicationRepository();
      }

      public void DispatchAlerts()
      {
         try
         {
            LOGGER.Info("Starting Alert Dispatch");

            var gracePeriodSetting = ConfigurationManager.AppSettings["grace_period_minutes"];
            var gracePeriodMinutes = !string.IsNullOrEmpty(gracePeriodSetting) ? int.Parse(gracePeriodSetting) : 2;

            var pendingAlerts = _applicationRepository.GetPendingAlerts(gracePeriodMinutes);
            if (pendingAlerts.Count == 0)
            {
               LOGGER.Info("No pending alerts. Alert Dispatch Complete");
               return;
            }

            // silenced (directly or through their group) after the failure was flagged: clear without emailing
            var processedIds = pendingAlerts.Where(a => a.Is_Silenced).Select(a => a.Id).ToList();
            if (processedIds.Count > 0)
               LOGGER.Info($"{processedIds.Count} pending alert(s) belong to silenced applications and will not be emailed.");

            // one digest per group, standalone applications last
            var groups = pendingAlerts
               .Where(a => !a.Is_Silenced)
               .GroupBy(a => a.Application_Group_Id ?? string.Empty)
               .OrderBy(g => g.Key == string.Empty ? 1 : 0)
               .ThenBy(g => g.First().Application_Group_Name);

            foreach (var group in groups)
            {
               var groupName = group.Key == string.Empty ? INDIVIDUAL_APPLICATIONS : group.First().Application_Group_Name;
               var apps = group.ToList();

               try
               {
                  if (SendGroupDigest(groupName, apps))
                  {
                     processedIds.AddRange(apps.Select(a => a.Id));
                     LOGGER.Info($"Alert digest sent for {groupName} ({apps.Count} component(s) down).");
                  }
                  else
                  {
                     LOGGER.Info($"Alert digest for {groupName} was not sent. Its alerts stay pending for the next run.");
                  }
               }
               catch (Exception ex)
               {
                  LOGGER.Info($"Alert digest for {groupName} has encountered an error: {ex.Message}");
                  LOGGER.Error(ex);
               }
            }

            _applicationRepository.ClearAlertPendingFlags(processedIds);
            LOGGER.Info("Alert Dispatch Complete");
         }
         catch (Exception ex)
         {
            LOGGER.Info("DispatchAlerts has encountered an error. Exception: " + ex.Message + "; Inner Exception: " + (ex.InnerException == null ? "" : ex.InnerException.Message));
            LOGGER.Error(ex);
         }
      }

      private bool SendGroupDigest(string groupName, List<ApplicationModel> apps)
      {
         var region = ConfigurationManager.AppSettings["region"];

         string MAIL_FROM = ConfigurationManager.AppSettings["mail_from"];
         string MAIL_TO = ConfigurationManager.AppSettings["mail_to"];
         string MAIL_CC = ConfigurationManager.AppSettings["mail_cc"];
         string MAIL_BCC = ConfigurationManager.AppSettings["mail_bcc"];

         string MAIL_SUBJECT = $"[{region}] Component Monitoring Alert: Group Alert - {groupName} ({apps.Count} Components Down)";

         using (MailMessage mailMsg = new MailMessage(MAIL_FROM, MailerHelper.FormatEmailAddresses(MAIL_TO), MAIL_SUBJECT, BuildDigestBody(groupName, apps)))
         {
            mailMsg.IsBodyHtml = true;
            if (!string.IsNullOrWhiteSpace(MAIL_CC)) mailMsg.CC.Add(MailerHelper.FormatEmailAddresses(MAIL_CC));
            if (!string.IsNullOrWhiteSpace(MAIL_BCC)) mailMsg.Bcc.Add(MailerHelper.FormatEmailAddresses(MAIL_BCC));

            return MailerHelper.Send(mailMsg);
         }
      }

      private string BuildDigestBody(string groupName, List<ApplicationModel> apps)
      {
         const string CELL = "style=\"border:1px solid #ccc;padding:6px 10px;text-align:left;vertical-align:top;\"";

         var body = new StringBuilder();
         body.AppendLine("<html><body>");
         body.AppendLine($"<p>The following components of <b>{Encode(groupName)}</b> are not working:</p>");
         body.AppendLine("<table style=\"border-collapse:collapse;font-family:Arial,sans-serif;font-size:13px;\">");
         body.AppendLine($"<tr style=\"background-color:#f2f2f2;\"><th {CELL}>Application Name</th><th {CELL}>Component Type</th><th {CELL}>Host Server</th><th {CELL}>Detection Time</th><th {CELL}>Details</th></tr>");

         foreach (var app in apps)
         {
            var detectedAt = app.Failure_Detected_At.HasValue ? DateTimeHelper.DisplayTzDateTime(app.Failure_Detected_At.Value) : "";

            body.AppendLine("<tr>"
               + $"<td {CELL}>{Encode(app.Application_Name)}</td>"
               + $"<td {CELL}>{Encode(app.Application_Type)}</td>"
               + $"<td {CELL}>{Encode(app.IP_Address)}</td>"
               + $"<td {CELL}>{Encode(detectedAt)}</td>"
               + $"<td {CELL}>{EncodeDetails(app.Failure_Details)}</td>"
               + "</tr>");
         }

         body.AppendLine("</table>");
         body.AppendLine("</body></html>");
         return body.ToString();
      }

      private static string Encode(string value) => WebUtility.HtmlEncode(value ?? string.Empty);

      // failure details come from the health checker and use <br/> for line breaks; everything else is encoded
      private static string EncodeDetails(string value) => Encode(value).Replace("&lt;br/&gt;", "<br/>");
   }
}
