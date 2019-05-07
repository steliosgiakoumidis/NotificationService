using NotificationCommon.Models;
using System;
using static NotificationCommon.Models.Enums;
using System.Globalization;

namespace NotificationService.Processing
{
    public static class RepetitionFrequencyRequirements
    {
        public static int WeekOfTheYear(DateTime datetime)
        {
            int weekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(datetime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekOfYear;
        }

        public static bool RepetitionFrequencySecondMonthRequirements(Sendout extraction)
        {
            var correctDayInMonth = extraction.StartDate.Day == DateTime.Now.Day;
            var rightDay = extraction.StartDate >= DateTime.Now;
            var hasLastRunAtValue = extraction.LastRunAt.HasValue
                                    ? DateTime.Now.Subtract(extraction.LastRunAt.Value.Date).Days != 0
                                    : true;
            var daysSinceLastRunAt = extraction.LastRunAt.HasValue
                                    ? (DateTime.Now.Subtract(extraction.LastRunAt.Value.Date).Days > 50)
                                    : true;
            var rightTime = DateTime.Now.Hour == Convert.ToInt16(extraction.ExecutionTime);

            return correctDayInMonth && rightDay && rightTime && hasLastRunAtValue && daysSinceLastRunAt;
        }

        public static bool RepetitionFrequencyMonthlyRequirements(Sendout extraction)
        {
            var correctDayInMonth = extraction.StartDate.Day == DateTime.Now.Day;
            var rightDay = extraction.StartDate >= DateTime.Now;
            var hasLastRunAtValue = extraction.LastRunAt.HasValue
                                    ? DateTime.Now.Subtract(extraction.LastRunAt.Value.Date).Days != 0
                                    : true;

            var rightTime = DateTime.Now.Hour == Convert.ToInt16(extraction.ExecutionTime);

            return correctDayInMonth && rightDay && hasLastRunAtValue & rightTime;
        }

        public static bool RepetitionFrequencyWeeklyRequirements(Sendout extraction)
        {
            var rightDay = extraction.DayOfTheWeek == WeekDay();
            var hasLastRunAtValue = extraction.LastRunAt.HasValue
            ? DateTime.Now.Subtract(extraction.LastRunAt.Value.Date).Days != 0
            : true;
            var startDayIsPAssed = extraction.StartDate >= DateTime.Now;
            var rightTime = DateTime.Now.Hour == Convert.ToInt16(extraction.ExecutionTime);

            return rightDay && hasLastRunAtValue && startDayIsPAssed && rightTime;
        }

        public static bool RepetitionFrequencyForthWeekRequirements(Sendout extraction,
            int startWeekNumber)
        {
            var rightDay = extraction.DayOfTheWeek == WeekDay();
            var hasLastRunAtValue = extraction.LastRunAt.HasValue
            ? DateTime.Now.Subtract(extraction.LastRunAt.Value.Date).Days != 0
            : true;
            var thisWeekNumber = WeekOfTheYear(DateTime.Now);
            var startDateIsPassed = extraction.StartDate <= DateTime.Now;
            var rightTime = DateTime.Now.Hour == Convert.ToInt16(extraction.ExecutionTime);
            var rightWeek = (thisWeekNumber - startWeekNumber) % 4 == 0;

            return rightDay && hasLastRunAtValue && startDateIsPassed && rightTime && rightWeek;
        }

        public static bool RepetitionFrequencySeconWeekRequirements(Sendout extraction,
        int startWeekNumber)
        {
            var correctDate = extraction.DayOfTheWeek == WeekDay();
            var hasLastRunAtValue = extraction.LastRunAt.HasValue
                                    ? DateTime.Now.Subtract(extraction.LastRunAt.Value.Date).Days != 0
                                    : true;
            var thisWeekNumber = WeekOfTheYear(DateTime.Now);
            var startDateIsPassed = extraction.StartDate <= DateTime.Now;
            var rightTime = DateTime.Now.Hour == Convert.ToInt16(extraction.ExecutionTime);
            var rightWeek = (thisWeekNumber - startWeekNumber) % 2 == 0;

            return correctDate && hasLastRunAtValue && startDateIsPassed && rightTime && rightWeek;
        }

        public static bool RepetitionFrequencySecondDayRequirements(Sendout extraction)
        {
            var hasLastRunAtValue = extraction.LastRunAt.HasValue
                                    ? DateTime.Now.Subtract(extraction.LastRunAt.Value.Date).Days == 2
                                    : true;
            var startDateIsPassed = extraction.StartDate <= DateTime.Now;
            var rightTime = DateTime.Now.Hour == Convert.ToInt16(extraction.ExecutionTime);
            return hasLastRunAtValue && startDateIsPassed && rightTime;
        }

        public static bool RepetitionFrequencyDailyRequirements(Sendout extraction)
        {
            var hasLastRunAtValue = extraction.LastRunAt.HasValue
             ? (DateTime.Now.Subtract(extraction.LastRunAt.Value.Date).Days) != 0 :
              true;
            var startDateIsPassed = extraction.StartDate.Date <= DateTime.Now.Date;
            var rightTime = DateTime.Now.Hour == Convert.ToInt16(extraction.ExecutionTime);
            return hasLastRunAtValue && startDateIsPassed && rightTime;
        }

        public static bool RepetitionFrequencyOnceRequirements(Sendout extraction)
        {
            var ItIsSameDate = DateTime.Now.Subtract(extraction.StartDate.Date).Days == 0;
            var HasLastRunAtValue = !extraction.LastRunAt.HasValue;
            var rightTime = DateTime.Now.Hour == Convert.ToInt16(extraction.ExecutionTime);

            return ItIsSameDate && HasLastRunAtValue && rightTime;
        }
        public static DayOfTheWeek WeekDay()
        {
            Enum.TryParse(DateTime.Now.DayOfWeek.ToString(), out DayOfTheWeek weekday);
            return weekday;
        }
    }
}
