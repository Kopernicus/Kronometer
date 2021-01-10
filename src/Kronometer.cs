/**
 * Kronometer - Makes the KSP clock easily editable
 * Copyright (c) 2017 Sigma88, Thomas P.
 * Licensed under the Terms of the MIT License
 */

using System;
using System.Text;
using UnityEngine;
using Kopernicus.ConfigParser;

namespace Kronometer
{
    // KRONOMETER STARTS HERE
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Kronometer : MonoBehaviour
    {
        /// <summary>
        /// Name of the config node group which manages Kronometer
        /// </summary>
        public const string rootNodeName = "Kronometer";

        void Start()
        {
            // Get the configNode
            ConfigNode kronometer = GameDatabase.Instance.GetConfigs(rootNodeName)[0].config;

            // Parse the config node
            SettingsLoader loader = Parser.CreateObjectFromConfigNode<SettingsLoader>(kronometer);

            if  // Make sure we need the clock and all values are defined properly
            (
                double.PositiveInfinity > loader.Clock.hour.value &&
                loader.Clock.hour.value > loader.Clock.minute.value &&
                loader.Clock.minute.value > loader.Clock.second.value &&
                loader.Clock.second.value > 0
            )
            {
                // Find the home planet
                CelestialBody homePlanet = FlightGlobals.GetHomeBody();

                // Get home planet day (rotation) and year (revolution)
                if (loader.useHomeDay)
                    loader.Clock.day.value = homePlanet.solarDayLength;
                if (loader.useHomeYear)
                    loader.Clock.year.value = homePlanet.orbitDriver.orbit.period;

                // If tidally locked set day = year
                if (loader.Clock.year.value == homePlanet.rotationPeriod)
                    loader.Clock.day.value = loader.Clock.year.value;

                // Convert negative numbers to positive
                loader.Clock.year.value = Math.Abs(loader.Clock.year.value);
                loader.Clock.day.value = Math.Abs(loader.Clock.day.value);

                // Fix Months
                if (loader.resetMonthNum == 0)
                    loader.resetMonthNum = loader.calendar?.Count ?? 1;

                // Round values where it is required
                if (loader.Clock.year.round)
                    loader.Clock.year.value = Math.Round(loader.Clock.year.value, 0);
                if (loader.Clock.day.round)
                    loader.Clock.day.value = Math.Round(loader.Clock.day.value, 0);
                if (loader.Clock.hour.round)
                    loader.Clock.hour.value = Math.Round(loader.Clock.hour.value, 0);
                if (loader.Clock.minute.round)
                    loader.Clock.minute.value = Math.Round(loader.Clock.minute.value, 0);
                if (loader.Clock.second.round)
                    loader.Clock.second.value = Math.Round(loader.Clock.second.value, 0);

                if  // Make sure we still need the clock and all values are still defined properly
                (
                    double.PositiveInfinity > loader.Clock.hour.value &&
                    loader.Clock.hour.value > loader.Clock.minute.value &&
                    loader.Clock.minute.value > loader.Clock.second.value &&
                    loader.Clock.second.value > 0
                )
                {
                    // Replace the stock Formatter
                    KSPUtil.dateTimeFormatter = new ClockFormatter(loader);
                }
            }
        }
    }

    // THIS IS THE REAL STUFF!
    public class ClockFormatter : IDateTimeFormatter
    {
        /// <summary>
        /// Required by IDateTimeFormatter
        /// </summary>
        public string PrintTime(double time, int valuesOfInterest, bool explicitPositive, bool logEnglish)
        {
            return PrintTime(time, valuesOfInterest, explicitPositive);
        }

        /// <summary>
        /// The object that contains the settings for the new clock
        /// </summary>
        protected SettingsLoader loader { get; set; }

        /// <summary>
        /// Create a new clock formatter
        /// </summary>
        public ClockFormatter(SettingsLoader loader)
        {
            this.loader = loader;
            FormatFixer();
        }

        /// <summary>
        /// Create a new clock formatter
        /// </summary>
        public ClockFormatter(ClockFormatter cloneFrom)
        {
            loader = cloneFrom.loader;
            FormatFixer();
        }

        // GET TIME
        // Splits seconds in years/days/hours/minutes/seconds
        protected int[] data = new int[6];

        /// <summary>
        /// This will count the number of Years, Days, Hours, Minutes and Seconds
        /// If a Year lasts 10.5 days, and time = 14 days, the result will be: 
        /// 1 Year, 3 days, and whatever hours-minutes-seconds fit in 0.5 dayloader.Clock.second.
        /// ( 10.5 + 3 + 0.5 = 14 )
        /// </summary>
        public virtual void GetTime(double time)
        {
            // Number of years
            int years = (int)(time / loader.Clock.year.value);

            // Time left to count
            double left = time - years * loader.Clock.year.value;

            // Number of days
            int days = (int)(left / loader.Clock.day.value);

            // Time left to count
            left = left - days * loader.Clock.day.value;

            // Number of hours
            int hours = (int)(left / loader.Clock.hour.value);

            // Time left to count
            left = left - hours * loader.Clock.hour.value;

            // Number of minutes
            int minutes = (int)(left / loader.Clock.minute.value);

            // Time left to count
            left = left - minutes * loader.Clock.minute.value;

            // Number of seconds
            int seconds = (int)(left / loader.Clock.second.value);

            data = new[] { 0, years, seconds, minutes, hours, days };
        }

        // PRINT TIME
        // Prints the time in the selected format

        public virtual string PrintTimeLong(double time)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            GetTime(time);
            StringBuilder sb = StringBuilderCache.Acquire();
            sb.Append(data[1]).Append(data[1] == 1 ? loader.Clock.year.singular : loader.Clock.year.plural).Append(", ");
            sb.Append(data[5]).Append(data[5] == 1 ? loader.Clock.day.singular : loader.Clock.day.plural).Append(", ");
            sb.Append(data[4]).Append(data[4] == 1 ? loader.Clock.hour.singular : loader.Clock.hour.plural).Append(", ");
            sb.Append(data[3]).Append(data[3] == 1 ? loader.Clock.minute.singular : loader.Clock.minute.plural).Append(", ");
            sb.Append(data[2]).Append(data[2] == 1 ? loader.Clock.second.singular : loader.Clock.second.plural);
            return sb.ToStringAndRelease();
        }

        public virtual string PrintTimeStamp(double time, bool days = false, bool years = false)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            GetTime(time);
            StringBuilder stringBuilder = StringBuilderCache.Acquire();

            if (years)
                stringBuilder.Append(loader.Clock.year.singular + " ").Append(data[1]).Append(", ");
            if (days)
                stringBuilder.Append("Day ").Append(data[5]).Append(" - ");

            stringBuilder.AppendFormat("{0:00}:{1:00}", data[4], data[3]);

            if (data[1] < 10)
                stringBuilder.AppendFormat(":{0:00}", data[2]);

            return stringBuilder.ToStringAndRelease();
        }

        public virtual string PrintTimeStampCompact(double time, bool days = false, bool years = false)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            GetTime(time);
            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            if (years)
                stringBuilder.Append(data[1]).Append(loader.Clock.year.symbol + ", ");

            if (days)
                stringBuilder.Append(data[5]).Append(loader.Clock.day.symbol + ", ");

            stringBuilder.AppendFormat("{0:00}:{1:00}", data[4], data[3]);
            if (data[1] < 10)
                stringBuilder.AppendFormat(":{0:00}", data[2]);

            return stringBuilder.ToStringAndRelease();
        }

        public virtual string PrintTime(double time, int valuesOfInterest, bool explicitPositive)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            bool flag = time < 0.0;
            GetTime(time);
            string[] symbols = { loader.Clock.second.symbol, loader.Clock.minute.symbol, loader.Clock.hour.symbol, loader.Clock.day.symbol, loader.Clock.year.symbol };
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

        public virtual string PrintTimeCompact(double time, bool explicitPositive)
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

        public virtual string PrintDateDelta(double time, bool includeTime, bool includeSeconds, bool useAbs)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            if (useAbs && time < 0.0)
                time = -time;

            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            GetTime(time);

            if (data[1] > 1)
                stringBuilder.Append(data[1]).Append(" " + loader.Clock.year.plural);
            else if (data[1] == 1)
                stringBuilder.Append(data[1]).Append(" " + loader.Clock.year.singular);

            if (data[5] > 1)
            {
                if (stringBuilder.Length != 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(data[5]).Append(" " + loader.Clock.day.plural);
            }
            else if (data[5] == 1)
            {
                if (stringBuilder.Length != 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(data[5]).Append(" " + loader.Clock.day.singular);
            }
            if (includeTime)
            {
                if (data[4] > 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(data[4]).Append(" " + loader.Clock.hour.plural);
                }
                else if (data[4] == 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(data[4]).Append(" " + loader.Clock.hour.singular);
                }
                if (data[3] > 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(data[3]).Append(" " + loader.Clock.minute.plural);
                }
                else if (data[3] == 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(data[3]).Append(" " + loader.Clock.minute.singular);
                }
                if (includeSeconds)
                {
                    if (data[2] > 1)
                    {
                        if (stringBuilder.Length != 0)
                            stringBuilder.Append(", ");
                        stringBuilder.Append(data[2]).Append(" " + loader.Clock.second.plural);
                    }
                    else if (data[2] == 1)
                    {
                        if (stringBuilder.Length != 0)
                            stringBuilder.Append(", ");
                        stringBuilder.Append(data[2]).Append(" " + loader.Clock.second.singular);
                    }
                }
            }
            if (stringBuilder.Length == 0)
                stringBuilder.Append((!includeTime) ? "0 " + loader.Clock.day.plural : ((!includeSeconds) ? "0 " + loader.Clock.minute.plural : "0 " + loader.Clock.second.plural));

            return stringBuilder.ToStringAndRelease();
        }

        public virtual string PrintDateDeltaCompact(double time, bool includeTime, bool includeSeconds, bool useAbs)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            if (useAbs && time < 0.0)
                time = -time;

            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            GetTime(time);
            if (data[1] > 0)
                stringBuilder.Append(data[1]).Append(loader.Clock.year.symbol);

            if (data[5] > 0)
            {
                if (stringBuilder.Length != 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(data[5]).Append(loader.Clock.day.symbol);
            }
            if (includeTime)
            {
                if (data[4] > 0)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(data[4]).Append(loader.Clock.hour.symbol);
                }
                if (data[3] > 0)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(data[3]).Append(loader.Clock.minute.symbol);
                }
                if (includeSeconds && data[2] > 0)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(data[2]).Append(loader.Clock.second.symbol);
                }
            }
            if (stringBuilder.Length == 0)
                stringBuilder.Append((!includeTime) ? "0" + loader.Clock.day.symbol : ((!includeSeconds) ? "0" + loader.Clock.minute.symbol : "0" + loader.Clock.second.symbol));
            return stringBuilder.ToStringAndRelease();
        }

        /// <summary>
        /// Calculates the current date
        /// This will work also when a year cannot be divided in days without a remainder
        /// If the year ends halfway through a day, the clock will go:
        /// Year 1 Day 365   ==>   Year 2 Day 1    (Hours, Minutes and Seconds do not reset)
        /// Day 1 will last untill Day 365 would have ended, then Day 2 will start.
        /// This way the time shown by the clock will always be consistent with the position of the sun in the sky
        /// </summary>
        public virtual Date GetDate(double time)
        {
            // Current Year
            int year = (int)Math.Floor(time / loader.Clock.year.value);

            // Time carried over each year
            double AnnualCarryOver = loader.Clock.year.value % loader.Clock.day.value;

            // Time carried over this year
            double CarryOverThisYear = MOD(AnnualCarryOver * year, loader.Clock.day.value);

            // Time passed this year
            double timeThisYear = MOD(time, loader.Clock.year.value) + CarryOverThisYear;

            // Current Day of the year
            int day = (int)Math.Floor(timeThisYear / loader.Clock.day.value.Value);

            // Time left to count
            double left = MOD(time, loader.Clock.day.value);

            // Number of hours in this day
            int hours = (int)(left / loader.Clock.hour.value);

            // Time left to count
            left = left - hours * loader.Clock.hour.value;

            // Number of minutes in this hour
            int minutes = (int)(left / loader.Clock.minute.value);

            // Time left to count
            left = left - minutes * loader.Clock.minute.value;

            // Number of seconds in this minute
            int seconds = (int)(left / loader.Clock.second.value);

            // Get Month
            Month month = null;
            int dayOfMonth = day;

            for (int i = 0; i < loader?.calendar?.Count; i++)
            {
                Month Mo = loader.calendar[i];

                month = Mo;

                if (dayOfMonth < Mo.days)
                    break;
                else if (Mo != loader.calendar[loader.calendar.Count - 1])
                    dayOfMonth -= Mo.days;
            }

            return new Date(year, month, dayOfMonth, day, hours, minutes, seconds);
        }

        /// <summary>
        /// Calculates the current date
        /// This will work also when a year cannot be divided in days without a remainder
        /// Every year will end at the last full day of the year.
        /// The time difference carries over from year to year, untill it's enough to get another full day.
        /// Example: if a year is 365.25 days, the first three years will have 365 days and 0.25 will carry over.
        /// On the fourth year there will be enough time carried over, so it will have 366 days.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public virtual Date GetLeapDate(double time)
        {
            // Time in a short (non-leap) year
            double shortYear = loader.Clock.year.value - (loader.Clock.year.value % loader.Clock.day.value);

            // Chance of getting a leap day in a year
            double chanceOfLeapDay = (loader.Clock.year.value % loader.Clock.day.value) / loader.Clock.day.value;

            // Number of days in a short (non-leap) year
            int daysInOneShortYear = (int)(shortYear / loader.Clock.day.value);

            // Time left to count
            double left = time;

            double leap = 0;
            int year = 0;
            int day = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            //Console.WriteLine("time = " + time);

            while (!(left < shortYear))
            {
                left -= shortYear;
                leap += chanceOfLeapDay;
                year += 1;
                while (!(leap < 1))
                {
                    leap -= 1;

                    if (!(left < loader.Clock.day.value))
                    {
                        left -= loader.Clock.day.value;
                    }
                    else
                    {
                        year -= 1;
                        day += daysInOneShortYear;
                    }
                }
            }

            day += (int)(left / loader.Clock.day.value);
            left -= (int)(left / loader.Clock.day.value) * loader.Clock.day.value;

            hours = (int)(left / 3600);
            left -= hours * 3600;

            minutes = (int)(left / 60);
            left -= minutes * 60;

            seconds = (int)left;

            // DATE CALCULATION COMPLETE

            // Temporary Month (needed later)
            Month month = null;
            int dayOfMonth = 0;

            // If there are months, change 'dayOfMonth' to indicate the current 'day of the month'
            if (loader.calendar.Count > 0)
            {
                // Calculate the time passed from the last month reset
                // Note: months reset every N years (Kronometer.resetMonths)

                // Total days passed untill now
                int daysPassedTOT = (int)Math.Floor(time / loader.Clock.day.value);

                // Days between month resets = normal days between month resets + leap days between month resets
                int daysBetweenResets = (daysInOneShortYear * loader.resetMonths) + (int)(chanceOfLeapDay * loader.resetMonths);

                // Days passed since last month reset
                int daysFromReset = (int)MOD(daysPassedTOT, daysBetweenResets);



                // Go through each month in the calendar
                for (int i = 0; i < loader?.calendar?.Count; i++)
                {
                    Month Mo = loader.calendar[i];

                    month = Mo;

                    // If there are more days left than there are in this month
                    // AND
                    // this is not the last month of the calendar
                    if (daysFromReset >= Mo.days && Mo != loader.calendar[loader.calendar.Count - 1])
                    {
                        // Remove this month worth of days and move on to check next month
                        daysFromReset -= Mo.days;
                    }
                    else break;
                    // If we run out of months, the last month will last until the next reset
                }

                // Set 'day' as 'day of the month'
                dayOfMonth = daysFromReset;
            }

            // The final date
            return new Date(year, month, dayOfMonth, day, hours, minutes, seconds);
        }


        // PRINT DATE
        // Prints the date in the selected format

        public virtual string PrintDate(double time, bool includeTime, bool includeSeconds = false)
        {
            // Check that the time is a meaningful number
            string text = CheckNum(time);

            if (text != null)
                return text;

            // The StringBuilder we will use to assemble the date
            StringBuilder stringBuilder = StringBuilderCache.Acquire();

            // Offset time
            time += loader.Display.CustomPrintDate.offsetTime;

            // Get the current date
            Date date = loader.useLeapYears ? GetLeapDate(time) : GetDate(time);

            // Offset years and days
            date.year += loader.Display.CustomPrintDate.offsetYear;
            date.day += loader.Display.CustomPrintDate.offsetDay;
            date.dayOfMonth += loader.Display.CustomPrintDate.offsetDay;

            // The format in which we will display the date
            string format = loader.Display.CustomPrintDate.displayDate;

            // Include time when necessary
            if (includeTime)
                format += loader.Display.CustomPrintDate.displayTime;

            // Include seconds when necessary
            if (includeSeconds)
                format += loader.Display.CustomPrintDate.displaySeconds;

            // Fix the syntax to .NET Framework composite formatting
            format = FormatFixer(format, date);

            // Create the date in the required format and return
            stringBuilder.AppendFormat(format, date.year, date.month != null ? date.month.Number(loader.calendar, loader.resetMonthNum).ToString() : "NaM", date.dayOfMonth, date.day, date.hours, date.minutes, date.seconds);

            return stringBuilder.ToStringAndRelease();
        }

        public virtual string PrintDateNew(double time, bool includeTime)
        {
            // Check that the time is a meaningful number
            string text = CheckNum(time);

            if (text != null)
                return text;

            // The StringBuilder we will use to assemble the date
            StringBuilder stringBuilder = StringBuilderCache.Acquire();

            // Offset time
            time += loader.Display.CustomPrintDateNew.offsetTime;

            // Get the current date
            Date date = loader.useLeapYears ? GetLeapDate(time) : GetDate(time);

            // Offset years and days
            date.year += loader.Display.CustomPrintDateNew.offsetYear;
            date.day += loader.Display.CustomPrintDateNew.offsetDay;

            // The format in which we will display the date
            string format = loader.Display.CustomPrintDateNew.displayDate;

            // Include time when necessary
            if (includeTime)
                format += loader.Display.CustomPrintDateNew.displayTime + loader.Display.CustomPrintDateNew.displaySeconds;

            // Fix the syntax to .NET Framework composite formatting
            format = FormatFixer(format, date);

            // Create the date in the required format and return
            stringBuilder.AppendFormat(format, date.year, date.month != null ? date.month.Number(loader.calendar, loader.resetMonthNum).ToString() : "NaM", date.day, date.hours, date.minutes, date.seconds);

            return stringBuilder.ToStringAndRelease();
        }

        public virtual string PrintDateCompact(double time, bool includeTime, bool includeSeconds = false)
        {
            // Check that the time is a meaningful number
            string text = CheckNum(time);

            if (text != null)
                return text;

            // The StringBuilder we will use to assemble the date
            StringBuilder stringBuilder = StringBuilderCache.Acquire();

            // Offset time
            time += loader.Display.CustomPrintDateCompact.offsetTime;

            // Get the current date
            Date date = loader.useLeapYears ? GetLeapDate(time) : GetDate(time);

            // Offset years and days
            date.year += loader.Display.CustomPrintDateCompact.offsetYear;
            date.day += loader.Display.CustomPrintDateCompact.offsetDay;

            // The format in which we will display the date
            string format = loader.Display.CustomPrintDateCompact.displayDate;

            // Include time when necessary
            if (includeTime)
                format += loader.Display.CustomPrintDateCompact.displayTime;

            // Include seconds when necessary
            if (includeSeconds)
                format += loader.Display.CustomPrintDateCompact.displaySeconds;

            // Fix the syntax to .NET Framework composite formatting
            format = FormatFixer(format, date);

            // Create the date in the required format and return
            stringBuilder.AppendFormat(format, date.year, date.month != null ? date.month.Number(loader.calendar, loader.resetMonthNum).ToString() : "NaM", date.day, date.hours, date.minutes, date.seconds);

            return stringBuilder.ToStringAndRelease();
        }

        /// <summary>
        /// Call FormatFixer on all 'DisplayLoader's 
        /// </summary>
        public void FormatFixer()
        {
            FormatFixer(loader.Display.CustomPrintDate);
            FormatFixer(loader.Display.CustomPrintDateNew);
            FormatFixer(loader.Display.CustomPrintDateCompact);
        }

        /// <summary>
        /// Fix the syntax to .NET Framework composite formatting
        /// This changes only the fixed parameters
        /// Some parameters will require to be changed live
        /// </summary>
        public virtual void FormatFixer(DisplayLoader display)
        {
            display.displayDate = FormatFixer(display.displayDate);
            display.displayTime = FormatFixer(display.displayTime);
            display.displaySeconds = FormatFixer(display.displaySeconds);
        }

        /// <summary>
        /// Replaces Kronometer syntax with .NET Framework composite formatting
        /// RULES:
        /// Angle brackets are used in place of curly brackets
        /// </summary>
        /// <Y> <Mo> <D> <M> <H> <S>         - number of the relative unit in the date
        /// <Y0> <D0> <M0> <H0> <S0>         - symbol of the relative unit
        /// <Y1> <D1> <M1> <H1> <S1>         - singular name of the relative unit
        public virtual string FormatFixer(string format)
        {
            return format

            // Fix Brackets
            .Replace("<", "{")
            .Replace(">", "}")
            .Replace("{{", "<")
            .Replace("}}", ">")
            .Replace("{ ", " ")
            .Replace(" }", " ")

            // Fix Years
            .Replace("{Y}", "{0}")
            .Replace("{Y:", "{0:")
            .Replace("{Y,", "{0,")
            .Replace("{Y0}", loader.Clock.year.symbol)
            .Replace("{Y1}", loader.Clock.year.singular)

            // Fix Months
            .Replace("{Mo}", "{1}")
            .Replace("{Mo:", "{1:")
            .Replace("{Dm}", "{2}")
            .Replace("{Dm:", "{2:")
            .Replace("{Dm,", "{2,")

            // Fix Days
            .Replace("{D}", "{3}")
            .Replace("{D:", "{3:")
            .Replace("{D,", "{3,")
            .Replace("{D0}", loader.Clock.day.symbol)
            .Replace("{D1}", loader.Clock.day.singular)

            // Fix Hours
            .Replace("{H}", "{4}")
            .Replace("{H:", "{4:")
            .Replace("{H,", "{4,")
            .Replace("{H0}", loader.Clock.hour.symbol)
            .Replace("{H1}", loader.Clock.hour.singular)

            // Fix Minutes
            .Replace("{M}", "{5}")
            .Replace("{M:", "{5:")
            .Replace("{M,", "{5,")
            .Replace("{M0}", loader.Clock.minute.symbol)
            .Replace("{M1}", loader.Clock.minute.singular)

            // Fix Seconds
            .Replace("{S}", "{6}")
            .Replace("{S:", "{6:")
            .Replace("{S,", "{6,")
            .Replace("{S0}", loader.Clock.second.symbol)
            .Replace("{S1}", loader.Clock.second.singular);
        }

        /// <summary>
        /// Translate Kopernicus syntax to .NET Framework composite formatting
        /// The syntax for these parameter is linked to their value
        /// for this reason they need to be changed changed live
        /// RULES:
        /// Angle brackets are used in place of curly brackets
        /// <Mo0> <Mo1>                  - symbol and name of the required month
        /// <Y2> <D2> <M2> <H2> <S2>     - plural name of the relative unit (uses singular when the number is 1)
        /// <Dth>                        - ordinal suffix for the number of the day ("st", "nd", "rd", "th")
        /// </summary>
        public virtual string FormatFixer(string format, Date date)
        {
            return format

            // Fix Plurals
            .Replace("{Y2}", date.year == 1 ? loader.Clock.year.singular : loader.Clock.year.plural)
            .Replace("{D2}", date.day == 1 ? loader.Clock.day.singular : loader.Clock.day.plural)
            .Replace("{H2}", date.hours == 1 ? loader.Clock.hour.singular : loader.Clock.hour.plural)
            .Replace("{M2}", date.minutes == 1 ? loader.Clock.minute.singular : loader.Clock.minute.plural)
            .Replace("{S2}", date.seconds == 1 ? loader.Clock.second.singular : loader.Clock.second.plural)

            // Fix Months
            .Replace("{Mo0}", date.month != null ? date.month.symbol : "NaM")
            .Replace("{Mo1}", date.month != null ? date.month.name : "NaM")

            // Fix Days
            .Replace("{Dth}", GetOrdinal(date.day));
        }

        /// <summary>
        /// From an integer input, outputs the correct ordinal suffix
        /// RULES:
        /// All numbers ending in '1' (except those ending in '11') get 'st'
        /// All numbers ending in '2' (except those ending in '12') get 'nd'
        /// All numbers ending in '3' (except those ending in '13') get 'rd'
        /// All remaining numbers get 'th'
        /// </summary>
        public virtual string GetOrdinal(int number)
        {
            string s = (number % 100).ToString();
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

        /// <summary>
        /// Check Num and Stock Properties
        /// </summary>
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

        /// <summary>
        /// Returns the remainder after number is divided by divisor. The result has the same sign as divisor.
        /// </summary>
        /// <param name="number">The number for which you want to find the remainder.</param>
        /// <param name="divisor">The number by which you want to divide number.</param>
        /// <returns></returns>
        public double MOD(double number, double divisor)
        {
            return (number % divisor + divisor) % divisor;
        }

        /// In these Properties is stored the length of each time unit in game seconds
        /// These can be found in stock as well, and should be used by other mods that deal with time
        public virtual int Second
        {
            get { return (int)loader.Clock.second.value; }
        }
        public virtual int Minute
        {
            get { return (int)loader.Clock.minute.value; }
        }
        public virtual int Hour
        {
            get { return (int)loader.Clock.hour.value; }
        }
        public virtual int Day
        {
            get { return (int)loader.Clock.day.value; }
        }
        public virtual int Year
        {
            get { return (int)loader.Clock.year.value; }
        }
    }

    /// <summary>
    /// Small class to handle dates easily
    /// </summary>
    public class Date
    {
        public int year { get; set; }
        public Month month { get; set; }
        public int dayOfMonth { get; set; }
        public int day { get; set; }
        public int hours { get; set; }
        public int minutes { get; set; }
        public int seconds { get; set; }

        public Date(int year, Month month, int dayOfMonth, int day, int hours, int minutes, int seconds)
        {
            this.year = year;
            this.day = day;
            this.dayOfMonth = dayOfMonth;
            this.month = month;
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
        }

        public Date(Date date)
        {
            year = date.year;
            month = date.month;
            dayOfMonth = date.dayOfMonth;
            day = date.day;
            hours = date.hours;
            minutes = date.minutes;
            seconds = date.seconds;
        }
    }
}
