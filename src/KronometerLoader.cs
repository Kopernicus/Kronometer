/**
 * Kronometer - Makes the KSP clock easily editable
 * Copyright (c) 2017 Sigma88, Thomas P.
 * Licensed under the Terms of the MIT License
 */

using System.Collections.Generic;
using Kopernicus;

namespace Kronometer
{
    // VARIOUS LOADERS FOR KRONOMETER SETTINGS
    [RequireConfigType(ConfigType.Node)]
    public class SettingsLoader : IParserEventSubscriber
    {
        // Load General Settings
        [ParserTarget("useHomeDay")]
        public NumericParser<bool> useHomeDay
        {
            set
            {
                if (value.value)
                    Kronometer.useHomeDay = value.value;
            }
        }

        [ParserTarget("useHomeYear")]
        public NumericParser<bool> useHomeYear
        {
            set
            {
                if (value.value)
                    Kronometer.useHomeYear = value.value;
            }
        }

        [ParserTarget("useLeapYears")]
        public NumericParser<bool> useLeapYears
        {
            set
            {
                if (value.value)
                    Kronometer.useLeapYears = value.value;
            }
        }



        // Load Custom Time
        [ParserTarget("CustomTime", allowMerge = true, optional = true)]
        public ClockLoader Clock = new ClockLoader();



        // Load Custom Months
        [ParserTargetCollection("Months", allowMerge = true)]
        public List<Month> calendar
        {
            set { ClockFormatter.calendar = new List<Month>(value); }
        }
        
        // This defines after how many months the number displayed will reset back to 1
        // This needs to load after the list of months
        [ParserTarget("resetMonthNumAfterMonths")]
        public NumericParser<int> resetMonthNum
        {
            set
            {
                if (value.value > 0)
                    Kronometer.resetMonthNum = value.value;
                else if (ClockFormatter.calendar.Count > 0)
                    Kronometer.resetMonthNum = ClockFormatter.calendar.Count;
                else
                    Kronometer.resetMonthNum = 1;
            }
        }
        
        // This defines after how many years the actual months will reset back to the first month
        [ParserTarget("resetMonthsAfterYears")]
        public NumericParser<int> resetMonths
        {
            set
            {
                if (value.value > 0)
                    Kronometer.resetMonths = value.value;
            }
        }



        // Load Custom Display
        [ParserTarget("DisplayDate", allowMerge = true, optional = true)]
        public CustomDisplay Display = new CustomDisplay();



        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            // Turn On The Kronometer
            Kronometer.useCustomClock = true;
        }

        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
        }

        public SettingsLoader()
        {
        }
    }


    // Loader for time units names, symbols, and durations
    [RequireConfigType(ConfigType.Node)]
    public class ClockLoader : IParserEventSubscriber
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



        void IParserEventSubscriber.Apply(ConfigNode node)
        {
        }

        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
            // Send time units definitions to the ClockFormatter
            ClockFormatter.S = second;
            ClockFormatter.M = minute;
            ClockFormatter.H = hour;
            ClockFormatter.D = day;
            ClockFormatter.Y = year;
        }

        public ClockLoader()
        {
        }
    }


    // Class to handle time units easily
    [RequireConfigType(ConfigType.Node)]
    public class TimeUnits : IParserEventSubscriber
    {
        [ParserTarget("singular")]
        public string singular;

        [ParserTarget("plural")]
        public string plural;

        [ParserTarget("symbol")]
        public string symbol;

        [ParserTarget("value")]
        public NumericParser<double> value;



        void IParserEventSubscriber.Apply(ConfigNode node)
        {
        }

        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
        }

        public TimeUnits(string singular, string plural, string symbol, double value)
        {
            this.singular = singular;
            this.plural = plural;
            this.symbol = symbol;
            this.value = value;
        }

        public TimeUnits()
        {
        }
    }


    // Loader for custom dates
    [RequireConfigType(ConfigType.Node)]
    public class CustomDisplay : IParserEventSubscriber
    {
        [ParserTarget("PrintDate", allowMerge = true)]
        public DisplayLoader CustomPrintDate = new DisplayLoader(1, 1, "<Y1> <Y>, <D1> <D>", " - <H><H0><M><M0>", ", <S><S0>");

        [ParserTarget("PrintDateNew", allowMerge = true)]
        public DisplayLoader CustomPrintDateNew = new DisplayLoader(1, 1, "<Y1> <Y>, <D1> <D>", " - <H:D2>:<M:D2>:<S:D2>", "");

        [ParserTarget("PrintDateCompact", allowMerge = true)]
        public DisplayLoader CustomPrintDateCompact = new DisplayLoader(1, 1, "<Y0><Y>, <D0><D:00>", ", <H>:<M:00>", ":<S:00>");



        void IParserEventSubscriber.Apply(ConfigNode node)
        {
        }

        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
            ClockFormatter.CustomPrintDate = CustomPrintDate;
            ClockFormatter.CustomPrintDateNew = CustomPrintDateNew;
            ClockFormatter.CustomPrintDateCompact = CustomPrintDateCompact;
        }

        public CustomDisplay()
        {
        }
    }


    // Loader for custom date display formats
    [RequireConfigType(ConfigType.Node)]
    public class DisplayLoader : IParserEventSubscriber
    {
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



        void IParserEventSubscriber.Apply(ConfigNode node)
        {
        }

        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
        }

        public DisplayLoader()
        {
        }

        public DisplayLoader(int offsetYear, int offsetDay, string displayDate, string displayTime, string displaySeconds)
        {
            this.offsetYear = offsetYear;
            this.offsetDay = offsetDay;
            this.displayDate = displayDate;
            this.displayTime = displayTime;
            this.displaySeconds = displaySeconds;
        }
    }


    // Small class to handle months
    [RequireConfigType(ConfigType.Node)]
    public class Month : IParserEventSubscriber
    {
        [ParserTarget("name")]
        public string name = "";

        [ParserTarget("symbol")]
        public string symbol = "";

        [ParserTarget("days")]
        public NumericParser<int> days = 0;


        // Get the number of this month
        public int number
        {
            get
            {
                if (ClockFormatter.calendar.Contains(this))
                    return (ClockFormatter.calendar.IndexOf(this) % Kronometer.resetMonthNum) + 1;
                else
                    return 0;
            }
        }



        void IParserEventSubscriber.Apply(ConfigNode node)
        {
        }

        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
        }

        public Month(string name, string symbol, int days)
        {
            this.name = name;
            this.symbol = symbol;
            this.days = days;
        }

        public Month()
        {
        }
    }
}
