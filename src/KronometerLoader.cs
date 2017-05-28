/**
 * Kronometer - Makes the KSP clock easily editable
 * Copyright (c) 2017 Sigma88, Thomas P.
 * Licensed under the Terms of the MIT License
 */

using System;
using System.Collections.Generic;
using Kopernicus;

namespace Kronometer
{
    /// <summary>
    /// Loads the settings that control Kronometer
    /// </summary>
    [RequireConfigType(ConfigType.Node)]
    public class SettingsLoader
    {
        // Whether to calculate the length of a day based on the orbital parameters of the home body
        [ParserTarget("useHomeDay")]
        public NumericParser<bool> useHomeDay = new NumericParser<bool>(false);

        // Calculates the length of a year based on the orbit of the home body
        [ParserTarget("useHomeYear")]
        public NumericParser<bool> useHomeYear = new NumericParser<bool>(false);

        // Automatically adds leap years if the day length and year length dont fit
        [ParserTarget("useLeapYears")]
        public NumericParser<bool> useLeapYears = new NumericParser<bool>(false);

        // Load Custom Time
        [ParserTarget("CustomTime", allowMerge = true, optional = true)]
        public ClockLoader Clock = new ClockLoader();

        // Load Custom Months
        [ParserTargetCollection("Months", allowMerge = true)]
        public List<Month> calendar = new List<Month>();

        // This defines after how many months the number displayed will reset back to 1
        // This needs to load after the list of months
        public Int32 resetMonthNum { get; set; }

        [ParserTarget("resetMonthNumAfterMonths")]
        private NumericParser<int> resetMonthNumLoader
        {
            set
            {
                if (value > 0)
                    resetMonthNum = value;
                else if (calendar.Count > 0)
                    resetMonthNum = calendar.Count;
                else
                    resetMonthNum = 1;
            }
        }

        // This defines after how many years the actual months will reset back to the first month
        public Int32 resetMonths = 1;

        [ParserTarget("resetMonthsAfterYears")]
        private NumericParser<int> resetMonthsLoader
        {
            set
            {
                if (value > 0)
                    resetMonths = value;
            }
        }

        // Load Custom Display
        [ParserTarget("DisplayDate", allowMerge = true, optional = true)]
        public CustomDisplay Display = new CustomDisplay();
    }

    /// <summary>
    /// Loader for time units names, symbols, and durations
    /// </summary>
    [RequireConfigType(ConfigType.Node)]
    public class ClockLoader
    {
        [ParserTarget("Second", allowMerge = true)]
        public TimeUnits second = new TimeUnits("Second", "Seconds", "s", 1);

        [ParserTarget("Minute", allowMerge = true)]
        public TimeUnits minute = new TimeUnits("Minute", "Minutes", "m", 60);

        [ParserTarget("Hour", allowMerge = true)]
        public TimeUnits hour = new TimeUnits("Hour", "Hours", "h", 3600);

        [ParserTarget("Day", allowMerge = true)]
        public TimeUnits day = new TimeUnits("Day", "Days", "d", 3600 * (GameSettings.KERBIN_TIME ? 6 : 24));

        [ParserTarget("Year", allowMerge = true)]
        public TimeUnits year = new TimeUnits("Year", "Years", "y", 3600 * (GameSettings.KERBIN_TIME ? 6 * 426 : 24 * 365));
    }

    /// <summary>
    /// Class to handle time units easily
    /// </summary>
    [RequireConfigType(ConfigType.Node)]
    public class TimeUnits
    {
        [ParserTarget("singular")]
        public string singular;

        [ParserTarget("plural")]
        public string plural;

        [ParserTarget("symbol")]
        public string symbol;

        [ParserTarget("value")]
        public NumericParser<double> value;

        public TimeUnits(string singular, string plural, string symbol, double value)
        {
            this.singular = singular;
            this.plural = plural;
            this.symbol = symbol;
            this.value = value;
        }
    }

    /// <summary>
    /// Loader for custom dates
    /// </summary>
    [RequireConfigType(ConfigType.Node)]
    public class CustomDisplay
    {
        [ParserTarget("PrintDate", allowMerge = true)]
        public DisplayLoader CustomPrintDate = new DisplayLoader(0, 1, 1, "<Y1> <Y>, <D1> <D>", " - <H><H0><M><M0>", ", <S><S0>");

        [ParserTarget("PrintDateNew", allowMerge = true)]
        public DisplayLoader CustomPrintDateNew = new DisplayLoader(0, 1, 1, "<Y1> <Y>, <D1> <D>", " - <H:D2>:<M:D2>:<S:D2>", "");

        [ParserTarget("PrintDateCompact", allowMerge = true)]
        public DisplayLoader CustomPrintDateCompact = new DisplayLoader(0, 1, 1, "<Y0><Y>, <D0><D:00>", ", <H>:<M:00>", ":<S:00>");
    }


    /// <summary>
    /// Loader for custom date display formats
    /// </summary>
    [RequireConfigType(ConfigType.Node)]
    public class DisplayLoader
    {
        [ParserTarget("offsetTime")]
        public NumericParser<int> offsetTime;

        [ParserTarget("offsetYear")]
        public NumericParser<int> offsetYear;

        [ParserTarget("offsetDay")]
        public NumericParser<int> offsetDay;

        [ParserTarget("displayDate")]
        public string displayDate;

        [ParserTarget("displayTime")]
        public string displayTime;

        [ParserTarget("displaySeconds")]
        public string displaySeconds;

        public DisplayLoader(int offsetTime, int offsetYear, int offsetDay, string displayDate, string displayTime, string displaySeconds)
        {
            this.offsetTime = offsetTime;
            this.offsetYear = offsetYear;
            this.offsetDay = offsetDay;
            this.displayDate = displayDate;
            this.displayTime = displayTime;
            this.displaySeconds = displaySeconds;
        }
    }


    /// <summary>
    ///  Small class to handle months
    /// </summary>
    [RequireConfigType(ConfigType.Node)]
    public class Month
    {
        [ParserTarget("name")]
        public string name = "";

        [ParserTarget("symbol")]
        public string symbol = "";

        [ParserTarget("days")]
        public NumericParser<int> days = 0;

        public Int32 Number(List<Month> calendar, Int32 resetMonthNum)
        {
            if (calendar.Contains(this))
                return (calendar.IndexOf(this) % resetMonthNum) + 1;
            return 0;
        }

        public Month()
        {
            name = "";
            symbol = "";
            days = 0;
        }

        public Month(string name, string symbol, int days)
        {
            this.name = name;
            this.symbol = symbol;
            this.days = days;
        }
    }
}
