using System;


namespace Music_Player.Utility
{
    internal static class DateAndTime
    {
        /// <summary>
        /// Returns date and time in readable format "DD-MM-YYYY | hh:mm:ss"
        /// </summary>
        /// <returns></returns>
        public static string GetNowDateAndTime()
        {
            DateTime dateTime = DateTime.Now;

            return
                $"{(dateTime.Day < 10 ? "0" + dateTime.Day.ToString() : dateTime.Day.ToString())}-" +
                $"{(dateTime.Month < 10 ? "0" + dateTime.Month.ToString() : dateTime.Month.ToString())}-{dateTime.Year} | " +
                $"{(dateTime.Hour < 10 ? "0" + dateTime.Hour.ToString() : dateTime.Hour.ToString())}:" +
                $"{(dateTime.Minute < 10 ? "0" + dateTime.Minute.ToString() : dateTime.Minute.ToString())}:" +
                $"{(dateTime.Second < 10 ? "0" + dateTime.Second.ToString() : dateTime.Second.ToString())}";
        }

        public static string GetMusicLength(TimeSpan timeSpan)
        {
            return (timeSpan.Hours > 0 ?
                   $"{timeSpan.Hours}:{(timeSpan.Minutes < 10 ? $"0{timeSpan.Minutes}" : $"{timeSpan.Minutes}")}:" : $"{timeSpan.Minutes}:")
                   + (timeSpan.Seconds < 10 ? $"0{timeSpan.Seconds}" : timeSpan.Seconds.ToString());
        }
    }
}