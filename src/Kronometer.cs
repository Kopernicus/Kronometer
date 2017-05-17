/**
 * Kronometer - Makes the KSP clock easily editable
 * Copyright (c) 2017 Sigma88, Thomas P.
 * Licensed under the Terms of the MIT License
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Kopernicus;

namespace Kronometer
{
    // KRONOMETER STARTS HERE
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Kronometer : MonoBehaviour
    {
        // Name of the config node group which manages Kronometer
        public const string rootNodeName = "Kronometer";

        // If Kronometer is needed, this will be 'true'
        public static bool useCustomClock = false;

        // Settings for time units and date management
        public static bool useHomeYear = false;
        public static bool useHomeDay = false;
        public static bool useLeapYears = false;

        // Settings for months
        public static int resetMonths = 1;
        public static int resetMonthNum = 0;


        public void Start()
        {
            // Get the configNode
            ConfigNode kronometer = GameDatabase.Instance.GetConfigs(rootNodeName)[0].config;

            // DAMN YOU THOMAS, I LOST HOURS TO FIND THAT I NEEDED THIS :D
            ParserOptions.Register("Kronometer", new ParserOptions.Data { errorCallback = e => Debug.Log(e), logCallback = s => Debug.Log(s) });

            // Parse the config node
            SettingsLoader loader = Parser.CreateObjectFromConfigNode<SettingsLoader>(kronometer, "Kronometer");


            if  // Make sure we need the clock and all values are defined properly
            (
                useCustomClock &&
                double.PositiveInfinity > ClockFormatter.H.value &&
                ClockFormatter.H.value > ClockFormatter.M.value &&
                ClockFormatter.M.value > ClockFormatter.S.value &&
                ClockFormatter.S.value > 0
            )
            {
                // Find the home planet
                CelestialBody homePlanet = FlightGlobals.GetHomeBody();

                // Get home planet day (rotation) and year (revolution)
                if (useHomeDay)
                    ClockFormatter.D.value = homePlanet.solarDayLength;
                if (useHomeYear)
                    ClockFormatter.Y.value = homePlanet.orbitDriver.orbit.period;

                // If tidally locked set day = year
                if (ClockFormatter.Y.value == homePlanet.rotationPeriod)
                    ClockFormatter.D.value = ClockFormatter.Y.value;

                // Convert negative numbers to positive
                ClockFormatter.Y.value = Math.Abs(ClockFormatter.Y.value);
                ClockFormatter.D.value = Math.Abs(ClockFormatter.D.value);

                // If weird numbers, abort
                if (double.IsInfinity(ClockFormatter.D.value) || double.IsNaN(ClockFormatter.D.value) || double.IsInfinity(ClockFormatter.Y.value) || double.IsNaN(ClockFormatter.Y.value))
                {
                    useCustomClock = false;
                }

                // Replace the stock Formatter
                if (useCustomClock)
                {
                    KSPUtil.dateTimeFormatter = new ClockFormatter();
                }
            }
        }
    }

    // THIS IS THE REAL STUFF!
    public class ClockFormatter : IDateTimeFormatter
    {
        // Date Time Formatter
        public static KSPUtil.DefaultDateTimeFormatter DTF = new KSPUtil.DefaultDateTimeFormatter();

        // Custom Time Units Definitions
        public static TimeUnits S = new TimeUnits("Second", "Seconds", "s", 1);
        public static TimeUnits M = new TimeUnits("Minute", "Minutes", "m", 60);
        public static TimeUnits H = new TimeUnits("Hour", "Hours", "h", 3600);
        public static TimeUnits D = new TimeUnits("Day", "Days", "d", 3600 * (GameSettings.KERBIN_TIME ? 6 : 24));
        public static TimeUnits Y = new TimeUnits("Year", "Years", "y", 3600 * (GameSettings.KERBIN_TIME ? 6 * 426 : 24 * 365));

        // Custom Calendar
        public static List<Month> calendar = new List<Month>();

        // Custom Formats to Display Dates
        public static DisplayLoader CustomPrintDate = new DisplayLoader(1, 1, "<Y1> <Y>, <D1> <D>", " - <H><H0><M><M0>", ", <S><S0>");
        public static DisplayLoader CustomPrintDateNew = new DisplayLoader(1, 1, "<Y1> <Y>, <D1> <D>", " - <H:D2>:<M:D2>:<S:D2>", "");
        public static DisplayLoader CustomPrintDateCompact = new DisplayLoader(1, 1, "<Y0><Y>, <D0><D:00>", ", <H>:<M:00>", ":<S:00>");



        // GET TIME
        // Splits seconds in years/days/hours/minutes/seconds

        public int[] data = new int[6];

        public void GetTime(double time)
        {
            // This will count the number of Years, Days, Hours, Minutes and Seconds
            // If a Year lasts 10.5 days, and time = 14 days, the result will be: 
            // 1 Year, 3 days, and whatever hours-minutes-seconds fit in 0.5 dayS.
            // ( 10.5 + 3 + 0.5 = 14 )

            // Number of years
            int years = (int)(time / Y.value);

            // Time left to count
            double left = time - years * Y.value;

            // Number of days
            int days = (int)(left / D.value);

            // Time left to count
            left = left - days * D.value;

            // Number of hours
            int hours = (int)(left / H.value);

            // Time left to count
            left = left - hours * H.value;

            // Number of minutes
            int minutes = (int)(left / M.value);

            // Time left to count
            left = left - minutes * M.value;

            // Number of seconds
            int seconds = (int)(left / S.value);

            data = new[] { 0, years, seconds, minutes, hours, days };
        }


        // PRINT TIME
        // Prints the time in the selected format

        public string PrintTimeLong(double time)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            GetTime(time);
            StringBuilder sb = StringBuilderCache.Acquire();
            sb.Append(data[1]).Append(data[1] == 1 ? Y.singular : Y.plural).Append(", ");
            sb.Append(data[5]).Append(data[5] == 1 ? D.singular : D.plural).Append(", ");
            sb.Append(data[4]).Append(data[4] == 1 ? H.singular : H.plural).Append(", ");
            sb.Append(data[3]).Append(data[3] == 1 ? M.singular : M.plural).Append(", ");
            sb.Append(data[2]).Append(data[2] == 1 ? S.singular : S.plural);
            return sb.ToStringAndRelease();
        }

        public string PrintTimeStamp(double time, bool days = false, bool years = false)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            GetTime(time);
            StringBuilder stringBuilder = StringBuilderCache.Acquire();

            if (years)
                stringBuilder.Append(Y.singular + " ").Append(data[1]).Append(", ");
            if (days)
                stringBuilder.Append("Day ").Append(data[5]).Append(" - ");

            stringBuilder.AppendFormat("{0:00}:{1:00}", data[4], data[3]);

            if (data[1] < 10)
                stringBuilder.AppendFormat(":{0:00}", data[2]);

            return stringBuilder.ToStringAndRelease();
        }

        public string PrintTimeStampCompact(double time, bool days = false, bool years = false)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            GetTime(time);
            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            if (years)
                stringBuilder.Append(data[1]).Append(Y.symbol + ", ");

            if (days)
                stringBuilder.Append(data[5]).Append(D.symbol + ", ");

            stringBuilder.AppendFormat("{0:00}:{1:00}", data[4], data[3]);
            if (data[1] < 10)
                stringBuilder.AppendFormat(":{0:00}", data[2]);

            return stringBuilder.ToStringAndRelease();
        }

        public string PrintTime(double time, int valuesOfInterest, bool explicitPositive)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            bool flag = time < 0.0;
            GetTime(time);
            string[] symbols = { S.symbol, M.symbol, H.symbol, D.symbol, Y.symbol };
            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            if (flag)
                stringBuilder.Append("- ");
            else if (explicitPositive)
                stringBuilder.Append("+ ");

            int[] list = { data[2], data[3], data[4], data[5], data[1] };
            int j = list.Length;
            while (j-- > 0)
            {
                if (list[j] != 0)
                {
                    for (int i = j; i > Mathf.Max(j - valuesOfInterest, -1); i--)
                    {
                        stringBuilder.Append(Math.Abs(list[i])).Append(symbols[i]);
                        if (i - 1 > Mathf.Max(j - valuesOfInterest, -1))
                            stringBuilder.Append(", ");
                    }
                    break;
                }
            }
            return stringBuilder.ToStringAndRelease();
        }

        public string PrintTimeCompact(double time, bool explicitPositive)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            GetTime(time);
            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            if (time < 0.0)
                stringBuilder.Append("T- ");
            else if (explicitPositive)
                stringBuilder.Append("T+ ");

            if (data[5] > 0)
                stringBuilder.Append(Math.Abs(data[5])).Append(":");

            stringBuilder.AppendFormat("{0:00}:{1:00}:{2:00}", data[4], data[3], data[2]);
            return stringBuilder.ToStringAndRelease();
        }

        public string PrintDateDelta(double time, bool includeTime, bool includeSeconds, bool useAbs)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            if (useAbs && time < 0.0)
                time = -time;

            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            GetTime(time);

            if (data[1] > 1)
                stringBuilder.Append(data[1]).Append(" " + Y.plural);
            else if (data[1] == 1)
                stringBuilder.Append(data[1]).Append(" " + Y.singular);

            if (data[5] > 1)
            {
                if (stringBuilder.Length != 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(data[5]).Append(" " + D.plural);
            }
            else if (data[5] == 1)
            {
                if (stringBuilder.Length != 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(data[5]).Append(" " + D.singular);
            }
            if (includeTime)
            {
                if (data[4] > 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(data[4]).Append(" " + H.plural);
                }
                else if (data[4] == 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(data[4]).Append(" " + H.singular);
                }
                if (data[3] > 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(data[3]).Append(" " + M.plural);
                }
                else if (data[3] == 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(data[3]).Append(" " + M.singular);
                }
                if (includeSeconds)
                {
                    if (data[2] > 1)
                    {
                        if (stringBuilder.Length != 0)
                            stringBuilder.Append(", ");
                        stringBuilder.Append(data[2]).Append(" " + S.plural);
                    }
                    else if (data[2] == 1)
                    {
                        if (stringBuilder.Length != 0)
                            stringBuilder.Append(", ");
                        stringBuilder.Append(data[2]).Append(" " + S.singular);
                    }
                }
            }
            if (stringBuilder.Length == 0)
                stringBuilder.Append((!includeTime) ? "0 " + D.plural : ((!includeSeconds) ? "0 " + M.plural : "0 " + S.plural));

            return stringBuilder.ToStringAndRelease();
        }

        public string PrintDateDeltaCompact(double time, bool includeTime, bool includeSeconds, bool useAbs)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            if (useAbs && time < 0.0)
                time = -time;

            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            GetTime(time);
            if (data[1] > 0)
                stringBuilder.Append(data[1]).Append(Y.symbol);

            if (data[5] > 0)
            {
                if (stringBuilder.Length != 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(data[5]).Append(D.symbol);
            }
            if (includeTime)
            {
                if (data[4] > 0)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(data[4]).Append(H.symbol);
                }
                if (data[3] > 0)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(data[3]).Append(M.symbol);
                }
                if (includeSeconds && data[2] > 0)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(data[2]).Append(S.symbol);
                }
            }
            if (stringBuilder.Length == 0)
                stringBuilder.Append((!includeTime) ? "0" + D.symbol : ((!includeSeconds) ? "0" + M.symbol : "0" + S.symbol));
            return stringBuilder.ToStringAndRelease();
        }



        // GET DATE
        // Calculates the current date

        public Date GetDate(double time)
        {
            // This will work also when a year cannot be divided in days without a remainder
            // If the year ends halfway through a day, the clock will go:
            // Year 1 Day 365   ==>   Year 2 Day 0    (Instead of starting directly with Day 1)
            // Day 0 will last untill Day 365 would have ended, then Day 1 will start.
            // This way the time shown by the clock will always be consistent with the position of the sun in the sky

            // Current Year
            int year = (int)(time / Y.value);

            // Current Day
            int day = (int)((time / D.value) - Math.Round(year * Y.value / D.value, 0, MidpointRounding.AwayFromZero));

            // Time left to count
            double left = time % D.value;

            // Number of hours in this day
            int hours = (int)(left / H.value);

            // Time left to count
            left = left - hours * H.value;

            // Number of minutes in this hour
            int minutes = (int)(left / M.value);

            // Time left to count
            left = left - minutes * M.value;

            // Number of seconds in this minute
            int seconds = (int)(left / S.value);

            // Get Month
            Month month = new Month();

            foreach (Month Mo in calendar)
            {
                month = Mo;

                if (day < Mo.days)
                    break;
                else if (Mo != calendar.Last())
                    day -= Mo.days;
            }

            return new Date(year, month, day, hours, minutes, seconds);
        }

        public Date GetLeapDate(double time)
        {
            // This will work also when a year cannot be divided in days without a remainder
            // Every year will end at the last full day of the year.
            // The time difference carries over from year to year, untill it's enough to get another full day.
            // Example: if a year is 365.25 days, the first three years will have 365 days and 0.25 will carry over.
            // On the fourth year there will be enough time carried over, so it will have 366 days.

            // Time in a short (non-leap) year
            double shortYear = Y.value - (Y.value % D.value);

            // Chance of getting a leap day in a year
            double chanceOfLeapDay = (Y.value % D.value) / D.value;

            // Number of days in a short (non-leap) year
            int daysInOneShortYear = (int)(shortYear / D.value);

            // Current Year (calculated as if there were no leap years)
            int year = (int)(time / shortYear);

            // Time left this year (calculated as if there were no leap years)
            double timeFromPreviousYears = year * daysInOneShortYear * D.value;
            double timeLeftThisYear = time - timeFromPreviousYears;

            // Current Day of the Year (calculated as if there were no leap years)
            int day = (int)(timeLeftThisYear / D.value);

            // Remove the days lost to leap years
            day -= (int)(chanceOfLeapDay * year);

            // If days go negative, borrow days from the previous year
            while (day < 0)
            {
                year--;
                day += (int)(Y.value / D.value) + 1;
            }

            // Now 'day' and 'year' correctly account for leap years



            // Time left to count
            double left = time % D.value;

            // Number of hours in this day
            int hours = (int)(left / H.value);

            // Time left to count
            left = left - hours * H.value;

            // Number of minutes in this hour
            int minutes = (int)(left / M.value);

            // Time left to count
            left = left - minutes * M.value;

            // Number of seconds in this minute
            int seconds = (int)(left / S.value);

            // DATE CALCULATION COMPLETE



            // Temporary Month (needed later)
            Month month = new Month();

            // If there are months, change 'day' to indicate the current 'day of the month'
            if (calendar.Count > 0)
            {
                // Calculate the time passed from the last month reset
                // Note: months reset every N years (Kronometer.resetMonths)

                // Total days passed untill now
                int daysPassedTOT = (int)(time / D.value);

                // Days between month resets = normal days between month resets + leap days between month resets
                int daysBetweenResets = (daysInOneShortYear * Kronometer.resetMonths) + (int)(chanceOfLeapDay * Kronometer.resetMonths);

                // Days passed since last month reset
                int daysFromReset = daysPassedTOT % daysBetweenResets;



                // Go through each month in the calendar
                foreach (Month Mo in calendar)
                {
                    month = Mo;

                    // If there are more days left than there are in this month
                    // AND
                    // this is not the last month of the calendar
                    if (daysFromReset >= Mo.days && Mo != calendar.Last())
                    {
                        // Remove this month worth of days and move on to check next month
                        daysFromReset -= Mo.days;
                    }
                    else break;
                    // If we run out of months, the last month will last until the next reset
                }

                // Set 'day' as 'day of the month'
                day = daysFromReset;
            }

            // The final date
            return new Date(year, month, day, hours, minutes, seconds);
        }


        // PRINT DATE
        // Prints the date in the selected format

        public string PrintDate(double time, bool includeTime, bool includeSeconds = false)
        {
            // Check that the time is a meaningful number
            string text = CheckNum(time);

            if (text != null)
                return text;

            // The StringBuilder we will use to assemble the date
            StringBuilder stringBuilder = StringBuilderCache.Acquire();

            // Get the current date
            Date date = Kronometer.useLeapYears ? GetLeapDate(time) : GetDate(time);

            // Offset years and days
            date.year += CustomPrintDate.offsetYear;
            date.day += CustomPrintDate.offsetDay;

            // The format in which we will display the date
            string format = CustomPrintDate.displayDate;

            // Include time when necessary
            if (includeTime)
                format += CustomPrintDate.displayTime;

            // Include seconds when necessary
            if (includeSeconds)
                format += CustomPrintDate.displaySeconds;

            // Fix the syntax to .NET Framework composite formatting
            format = FormatFixer(format, date);

            // Create the date in the required format and return
            stringBuilder.AppendFormat(format, date.year, date.month.number, date.day, date.hours, date.minutes, date.seconds);

            return stringBuilder.ToStringAndRelease();
        }

        public string PrintDateNew(double time, bool includeTime)
        {
            // Check that the time is a meaningful number
            string text = CheckNum(time);

            if (text != null)
                return text;

            // The StringBuilder we will use to assemble the date
            StringBuilder stringBuilder = StringBuilderCache.Acquire();

            // Get the current date
            Date date = Kronometer.useLeapYears ? GetLeapDate(time) : GetDate(time);

            // Offset years and days
            date.year += CustomPrintDateNew.offsetYear;
            date.day += CustomPrintDateNew.offsetDay;

            // The format in which we will display the date
            string format = CustomPrintDateNew.displayDate;

            // Include time when necessary
            if (includeTime)
                format += CustomPrintDateNew.displayTime + CustomPrintDateNew.displaySeconds;

            // Fix the syntax to .NET Framework composite formatting
            format = FormatFixer(format, date);

            // Create the date in the required format and return
            stringBuilder.AppendFormat(format, date.year, date.month.number, date.day, date.hours, date.minutes, date.seconds);

            return stringBuilder.ToStringAndRelease();
        }

        public string PrintDateCompact(double time, bool includeTime, bool includeSeconds = false)
        {
            // Check that the time is a meaningful number
            string text = CheckNum(time);

            if (text != null)
                return text;

            // The StringBuilder we will use to assemble the date
            StringBuilder stringBuilder = StringBuilderCache.Acquire();

            // Get the current date
            Date date = Kronometer.useLeapYears ? GetLeapDate(time) : GetDate(time);

            // Offset years and days
            date.year += CustomPrintDateCompact.offsetYear;
            date.day += CustomPrintDateCompact.offsetDay;

            // The format in which we will display the date
            string format = CustomPrintDateCompact.displayDate;

            // Include time when necessary
            if (includeTime)
                format += CustomPrintDateCompact.displayTime;

            // Include seconds when necessary
            if (includeSeconds)
                format += CustomPrintDateCompact.displaySeconds;

            // Fix the syntax to .NET Framework composite formatting
            format = FormatFixer(format, date);

            // Create the date in the required format and return
            stringBuilder.AppendFormat(format, date.year, date.month.number, date.day, date.hours, date.minutes, date.seconds);

            return stringBuilder.ToStringAndRelease();
        }



        // FIX FORMAT SYNTAX
        // Translate Kopernicus syntax to .NET Framework composite formatting

        public void FormatFixer()
        {
            // Call FormatFixer on all 'DisplayLoader's 
            FormatFixer(CustomPrintDate);
            FormatFixer(CustomPrintDateNew);
            FormatFixer(CustomPrintDateCompact);
        }

        public void FormatFixer(DisplayLoader display)
        {
            // Fix the syntax to .NET Framework composite formatting
            // This changes only the fixed parameters
            // Some parameters will require to be changed live
            display.displayDate = FormatFixer(display.displayDate);
            display.displayTime = FormatFixer(display.displayTime);
            display.displaySeconds = FormatFixer(display.displaySeconds);
        }

        public string FormatFixer(string format)
        {
            // Replaces Kronometer syntax with .NET Framework composite formatting
            // RULES:
            // Angle brackets are used in place of curly brackets
            // <Y> <Mo> <D> <M> <H> <S>         - number of the relative unit in the date
            // <Y0> <D0> <M0> <H0> <S0>         - symbol of the relative unit
            // <Y1> <D1> <M1> <H1> <S1>         - singular name of the relative unit

            return format

            // Fix Brackets
            .Replace("<", "{")
            .Replace(">", "}")
            .Replace("{{", "<")
            .Replace("}}", ">")

            // Fix Years
            .Replace("{Y}", "{0}")
            .Replace("{Y:", "{0:")
            .Replace("{Y,", "{0,")
            .Replace("{Y0}", Y.symbol)
            .Replace("{Y1}", Y.singular)

            // Fix Months
            .Replace("{Mo}", "{1}")
            .Replace("{Mo:", "{1:")

            // Fix Days
            .Replace("{D}", "{2}")
            .Replace("{D:", "{2:")
            .Replace("{D,", "{2,")
            .Replace("{D0}", D.symbol)
            .Replace("{D1}", D.singular)

            // Fix Hours
            .Replace("{H}", "{3}")
            .Replace("{H:", "{3:")
            .Replace("{H,", "{3,")
            .Replace("{H0}", H.symbol)
            .Replace("{H1}", H.singular)

            // Fix Minutes
            .Replace("{M}", "{4}")
            .Replace("{M:", "{4:")
            .Replace("{M,", "{4,")
            .Replace("{M0}", M.symbol)
            .Replace("{M1}", M.singular)

            // Fix Seconds
            .Replace("{S}", "{5}")
            .Replace("{S:", "{5:")
            .Replace("{S,", "{5,")
            .Replace("{S0}", S.symbol)
            .Replace("{S1}", S.singular);
        }

        public string FormatFixer(string format, Date date)
        {
            // The syntax for these parameter is linked to their value
            // for this reason they need to be changed changed live
            // RULES:
            // Angle brackets are used in place of curly brackets
            // <Mo0> <Mo1>                  - symbol and name of the required month
            // <Y2> <D2> <M2> <H2> <S2>     - plural name of the relative unit (uses singular when the number is 1)
            // <Dth>                        - ordinal suffix for the number of the day ("st", "nd", "rd", "th")

            return format

            // Fix Plurals
            .Replace("{Y2}", date.year == 1 ? Y.singular : Y.plural)
            .Replace("{D2}", date.day == 1 ? D.singular : D.plural)
            .Replace("{H2}", date.hours == 1 ? H.singular : H.plural)
            .Replace("{M2}", date.minutes == 1 ? M.singular : M.plural)
            .Replace("{S2}", date.seconds == 1 ? S.singular : S.plural)

            // Fix Months
            .Replace("{Mo0}", date.month.symbol)
            .Replace("{Mo1}", date.month.name)

            // Fix Days
            .Replace("{Dth}", GetOrdinal(date.day));
        }

        public string GetOrdinal(int number)
        {
            // From an integer input, outputs the correct ordinal suffix
            // RULES:
            // All numbers ending in '1' except '11' get 'st'
            // All numbers ending in '2' except '12' get 'nd'
            // All numbers ending in '3' except '13' get 'rd'
            // All remaining numbers get 'th'


            string s = number.ToString();

            if (s.Length == 1 || (s.Length > 1 && s[s.Length - 2] != '1'))
            {
                if (s[s.Length - 1] == '1')
                    return "st";
                if (s[s.Length - 1] == '2')
                    return "nd";
                if (s[s.Length - 1] == '3')
                    return "rd";
            }

            return "th";
        }



        // Check Num and Stock Properties

        private static string CheckNum(double time)
        {
            if (double.IsNaN(time))
                return "NaN";

            if (double.IsPositiveInfinity(time))
                return "+Inf";

            if (double.IsNegativeInfinity(time))
                return "-Inf";

            return null;
        }

        public int Second
        {
            get { return (int)S.value; }
        }
        public int Minute
        {
            get { return (int)M.value; }
        }
        public int Hour
        {
            get { return (int)H.value; }
        }
        public int Day
        {
            get { return (int)D.value; }
        }
        public int Year
        {
            get { return (int)Y.value; }
        }


        // Call FormatFixer when creating the ClockFormatter
        public ClockFormatter()
        {
            FormatFixer();
        }
    }

    // Small class to handle dates easily
    public class Date
    {
        public int year { get; set; }
        public Month month { get; set; }
        public int day { get; set; }
        public int hours { get; set; }
        public int minutes { get; set; }
        public int seconds { get; set; }

        public Date()
        {
        }

        public Date(int year, Month month, int day, int hours, int minutes, int seconds)
        {
            this.year = year;
            this.day = day;
            this.month = month;
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
        }

        public Date(Date date)
        {
            year = date.year;
            day = date.day;
            month = date.month;
            hours = date.hours;
            minutes = date.minutes;
            seconds = date.seconds;
        }
    }
}
