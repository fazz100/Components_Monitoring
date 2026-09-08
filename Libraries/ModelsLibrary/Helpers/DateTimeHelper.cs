using System;

namespace ModelsLibrary.Helpers
{
   public class DateTimeHelper
   {
      // private
      private static string TIMEZONE_ID = "Eastern Standard Time";

      private static string dateFormat = "yyyy-MM-dd";
      //private static string timeFormat = "hh:mm tt ";
      private static string timeFormat = "HH:mm ";
      private static string dateTimeFormat = dateFormat + " " + timeFormat;

      private static string jsDateFormat = "yy-mm-dd";
      //private static string jsTimeFormat = "hh:mm tt";
      private static string jsTimeFormat = "HH:mm ";
      private static string jsDateTimeFormat = jsDateFormat + " " + jsTimeFormat;

      // public properties
      public static string DateFormat { get { return dateFormat; } }
      public static string TimeFormat { get { return timeFormat; } }
      public static string DateTimeFormat { get { return dateTimeFormat; } }

      public static string JsDateFormat { get { return jsDateFormat; } }
      public static string JsTimeFormat { get { return jsTimeFormat; } }
      public static string JsDateTimeFormat { get { return jsDateFormat + " " + jsTimeFormat; } }

      public static DateTime ConvertFromUtc(DateTime utcDate)
      {
         TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(TIMEZONE_ID);
         DateTime returnDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, timeZone);

         return returnDate;
      }

      public static DateTime ConvertFromTimeZone(DateTime dateTime)
      {
         TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(TIMEZONE_ID);
         DateTime returnDate = TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone);

         return returnDate;
      }

      public static string DisplayTzDateTime(DateTime utcDateTime)
      {
         if (utcDateTime != DateTime.MinValue)
         {
            return (ConvertFromUtc(utcDateTime).ToString(dateTimeFormat)) + " EST";
         }
         else
            return "";
      }

      

      public static DateTime ConvertFromUtc(DateTime utcDateTime, string timeZoneId)
      {
         TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
         DateTime returnDate = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timezone);

         return returnDate;
      }

      public static string DisplayTzDateTime2(DateTime utcDateTime)
      {
         if (utcDateTime != DateTime.MinValue)
         {
            return (ConvertFromUtc(utcDateTime).ToString(dateTimeFormat));
         }
         else
            return "";
      }

      public static string DisplayTzDateTime3(DateTime utcDateTime)
      {
         if (utcDateTime != DateTime.MinValue)
         {
            return (ConvertFromUtc(utcDateTime).ToString("MM/dd/yyyy HH:mm ")) + " Eastern";
         }
         else
            return "";
      }

      public static string DisplayTzDate(DateTime utcDateTime)
      {
         if (utcDateTime != DateTime.MinValue)
         {
            return ConvertFromUtc(utcDateTime).ToString(dateFormat);
         }
         else
            return "";
      }

      public static string DisplayTzTime(DateTime utcDateTime)
      {
         if (utcDateTime != DateTime.MinValue)
         {
            return ConvertFromUtc(utcDateTime).ToString(timeFormat);
         }
         else
            return "";
      }

      public static DateTime ConvertESTtoGMT(DateTime tzDate)
      {
         DateTime utcDate = ConvertFromTimeZone(tzDate);
         DateTime gmtDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));

         return gmtDate;
      }

      public static TimeSpan ParseTimeString(string time)
      {
         TimeSpan defaultTimeSpan = new TimeSpan(0, 0, 0);
         DateTime parsedValue;
         bool parsedSuccessfully;

         if (string.IsNullOrEmpty(time))
         {
            return defaultTimeSpan;
         }
         else if (time.Length == 3)
         {
            string modifiedTime = "0" + time;
            parsedSuccessfully = DateTime.TryParseExact(modifiedTime, "Hmm", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out parsedValue);
         }
         else if (time.Length == 4)
         {
            parsedSuccessfully = DateTime.TryParseExact(time, "HHmm", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out parsedValue);
         }
         else if (time.Length == 5)
         {
            parsedSuccessfully = DateTime.TryParseExact(time, "HH:mm", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out parsedValue);
            if (!parsedSuccessfully)
            {
               string modifiedTime = "0" + time;
               parsedSuccessfully = DateTime.TryParseExact(modifiedTime, "Hmmss", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out parsedValue);
            }
         }
         else if (time.Length == 6)
         {
            parsedSuccessfully = DateTime.TryParseExact(time, "HHmmss", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out parsedValue);
         }
         else if (time.Length == 8)
         {
            parsedSuccessfully = DateTime.TryParseExact(time, "HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out parsedValue);
         }
         else
         {
            return defaultTimeSpan;
         }

         if (parsedSuccessfully)
            return parsedValue.TimeOfDay;
         else
            return defaultTimeSpan;
      }
   }
}
