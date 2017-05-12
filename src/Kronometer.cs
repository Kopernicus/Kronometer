/**
 * Kronometer - Makes the KSP clock easily editable
 * Copyright (c) 2017 Sigma88, Thomas P.
 * Licensed under the Terms of the MIT License
 */

using System;
using System.Text;
using System.Linq;
using UnityEngine;
using Kopernicus;

namespace Kronometer
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Kronometer : MonoBehaviour
    {
        /// <summary>
        /// Name of the config node group which manages Kopernicus
        /// </summary>
        public const String rootNodeName = "Kronometer";

        /// <summary>
        /// Load the settings file from GameDatabase and init the clock formatter
        /// </summary>
        public void Start()
        {
            // Get the configNode
            ConfigNode kronometer = GameDatabase.Instance.GetConfigs(rootNodeName)[0].config;

            // Parse the config node and create a settings object
            ClockLoader loader = Parser.CreateObjectFromConfigNode<ClockLoader>(kronometer);

            if  // Make sure the customClock needs to be used, and all values are defined properly
            (
                Double.PositiveInfinity > loader.hour.value &&
                loader.hour.value > loader.minute.value &&
                loader.minute.value > loader.second.value &&
                loader.second.value > 0
            )
            {
                // Find the home planet
                CelestialBody homePlanet = FlightGlobals.Bodies.First(b => b.isHomeWorld);

                // Get custom year and day duration
                if (loader.useOrbitalParams)
                {
                    loader.year.value = homePlanet.orbitDriver.orbit.period;
                    loader.day.value = homePlanet.solarDayLength;

                    // If tidally locked set day = year
                    if (loader.year.value == homePlanet.rotationPeriod)
                        loader.day.value = loader.year.value;
                }

                // Convert negative numbers to positive
                if (loader.year.value < 0)
                    loader.year.value = -loader.year.value;
                if (loader.day.value < 0)
                    loader.day.value = -loader.day.value;

                // If weird numbers, abort
                if (Double.IsInfinity(loader.day.value) || Double.IsNaN(loader.day.value) || Double.IsInfinity(loader.year.value) || Double.IsNaN(loader.year.value))
                {
                    return;
                }

                // Replace the stock Formatter
                KSPUtil.dateTimeFormatter = new ClockFormatter(loader);
            }
        }
    }

    public class ClockFormatter : IDateTimeFormatter
    {
        /// <summary>
        /// The object that contains the settings for the new clock
        /// </summary>
        protected ClockLoader loader { get; set; }

        /// <summary>
        /// Create a new clock formatter
        /// </summary>
        public ClockFormatter(ClockLoader loader)
        {
            this.loader = loader;
        }

        /// <summary>
        /// Create a new clock formatter
        /// </summary>
        public ClockFormatter(ClockFormatter cloneFrom)
        {
            loader = cloneFrom.loader;
        }

        /// <summary>
        /// Caches values from time calculations
        /// </summary>
        public Int32[] cache = new Int32[6];

        public virtual String PrintTimeLong(Double time)
        {
            String text = CheckNum(time);
            if (text != null)
                return text;

            GetTime(time);
            StringBuilder sb = StringBuilderCache.Acquire();
            sb.Append(cache[1]).Append(cache[1] == 1 ? loader.year.singular : loader.year.plural).Append(", ");
            sb.Append(cache[5]).Append(cache[5] == 1 ? loader.day.singular : loader.day.plural).Append(", ");
            sb.Append(cache[4]).Append(cache[4] == 1 ? loader.hour.singular : loader.hour.plural).Append(", ");
            sb.Append(cache[3]).Append(cache[3] == 1 ? loader.minute.singular : loader.minute.plural).Append(", ");
            sb.Append(cache[2]).Append(cache[2] == 1 ? loader.second.singular : loader.second.plural);
            return sb.ToStringAndRelease();
        }

        public virtual String PrintTimeStamp(Double time, Boolean days = false, Boolean years = false)
        {
            String text = CheckNum(time);
            if (text != null)
                return text;

            GetTime(time);
            StringBuilder stringBuilder = StringBuilderCache.Acquire();

            if (years)
                stringBuilder.Append(loader.year.singular + " ").Append(cache[1]).Append(", ");
            if (days)
                stringBuilder.Append("Day ").Append(cache[5]).Append(" - ");

            stringBuilder.AppendFormat("{0:00}:{1:00}", cache[4], cache[3]);

            if (cache[1] < 10)
                stringBuilder.AppendFormat(":{0:00}", cache[2]);

            return stringBuilder.ToStringAndRelease();
        }

        public virtual String PrintTimeStampCompact(Double time, Boolean days = false, Boolean years = false)
        {
            String text = CheckNum(time);
            if (text != null)
                return text;

            GetTime(time);
            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            if (years)
                stringBuilder.Append(cache[1]).Append(loader.year.symbol + ", ");

            if (days)
                stringBuilder.Append(cache[5]).Append(loader.day.symbol + ", ");

            stringBuilder.AppendFormat("{0:00}:{1:00}", cache[4], cache[3]);
            if (cache[1] < 10)
                stringBuilder.AppendFormat(":{0:00}", cache[2]);

            return stringBuilder.ToStringAndRelease();
        }

        public virtual String PrintTime(Double time, Int32 valuesOfInterest, Boolean explicitPositive)
        {
            String text = CheckNum(time);
            if (text != null)
                return text;

            Boolean flag = time < 0.0;
            GetTime(time);
            String[] symbols = { loader.second.symbol, loader.minute.symbol, loader.hour.symbol, loader.day.symbol, loader.year.symbol };
            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            if (flag)
                stringBuilder.Append("- ");
            else if (explicitPositive)
                stringBuilder.Append("+ ");

            Int32[] list = { cache[2], cache[3], cache[4], cache[5], cache[1] };
            Int32 j = list.Length;
            while (j-- > 0)
            {
                if (list[j] != 0)
                {
                    for (Int32 i = j; i > Mathf.Max(j - valuesOfInterest, -1); i--)
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

        public virtual String PrintTimeCompact(Double time, Boolean explicitPositive)
        {
            String text = CheckNum(time);
            if (text != null)
                return text;
            
            GetTime(time);
            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            if (time < 0.0)
                stringBuilder.Append("T- ");
            else if (explicitPositive)
                stringBuilder.Append("T+ ");

            if (cache[5] > 0)
                stringBuilder.Append(Math.Abs(cache[5])).Append(":");

            stringBuilder.AppendFormat("{0:00}:{1:00}:{2:00}", cache[4], cache[3], cache[2]);
            return stringBuilder.ToStringAndRelease();
        }

        public virtual String PrintDateDelta(Double time, Boolean includeTime, Boolean includeSeconds, Boolean useAbs)
        {
            String text = CheckNum(time);
            if (text != null)
                return text;

            if (useAbs && time < 0.0)
                time = -time;

            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            GetTime(time);

            if (cache[1] > 1)
                stringBuilder.Append(cache[1]).Append(" " + loader.year.plural);
            else if (cache[1] == 1)
                stringBuilder.Append(cache[1]).Append(" " + loader.year.singular);

            if (cache[5] > 1)
            {
                if (stringBuilder.Length != 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(cache[5]).Append(" " + loader.day.plural);
            }
            else if (cache[5] == 1)
            {
                if (stringBuilder.Length != 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(cache[5]).Append(" " + loader.day.singular);
            }
            if (includeTime)
            {
                if (cache[4] > 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(cache[4]).Append(" " + loader.hour.plural);
                }
                else if (cache[4] == 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(cache[4]).Append(" " + loader.hour.singular);
                }
                if (cache[3] > 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(cache[3]).Append(" " + loader.minute.plural);
                }
                else if (cache[3] == 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(cache[3]).Append(" " + loader.minute.singular);
                }
                if (includeSeconds)
                {
                    if (cache[2] > 1)
                    {
                        if (stringBuilder.Length != 0)
                            stringBuilder.Append(", ");
                        stringBuilder.Append(cache[2]).Append(" " + loader.second.plural);
                    }
                    else if (cache[2] == 1)
                    {
                        if (stringBuilder.Length != 0)
                            stringBuilder.Append(", ");
                        stringBuilder.Append(cache[2]).Append(" " + loader.second.singular);
                    }
                }
            }
            if (stringBuilder.Length == 0)
                stringBuilder.Append((!includeTime) ? "0 " + loader.day.plural : ((!includeSeconds) ? "0 " + loader.minute.plural : "0 " + loader.second.plural));

            return stringBuilder.ToStringAndRelease();
        }

        public virtual String PrintDateDeltaCompact(Double time, Boolean includeTime, Boolean includeSeconds, Boolean useAbs)
        {
            String text = CheckNum(time);
            if (text != null)
                return text;

            if (useAbs && time < 0.0)
                time = -time;

            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            GetTime(time);
            if (cache[1] > 0)
                stringBuilder.Append(cache[1]).Append(loader.year.symbol);

            if (cache[5] > 0)
            {
                if (stringBuilder.Length != 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(cache[5]).Append(loader.day.symbol);
            }
            if (includeTime)
            {
                if (cache[4] > 0)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(cache[4]).Append(loader.hour.symbol);
                }
                if (cache[3] > 0)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(cache[3]).Append(loader.minute.symbol);
                }
                if (includeSeconds && cache[2] > 0)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(cache[2]).Append(loader.second.symbol);
                }
            }
            if (stringBuilder.Length == 0)
                stringBuilder.Append((!includeTime) ? "0" + loader.day.symbol : ((!includeSeconds) ? "0" + loader.minute.symbol : "0" + loader.second.symbol));
            return stringBuilder.ToStringAndRelease();
        }

        public virtual String PrintDate(Double time, Boolean includeTime, Boolean includeSeconds = false)
        {
            String text = CheckNum(time);

            if (text != null)
                return text;


            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            GetDate(time);

            stringBuilder.Append(loader.year.singular + " ").Append(cache[1] + 1).Append(", " + loader.day.singular + " ").Append(cache[5] + 1);
            if (includeTime)
                stringBuilder.Append(" - ").Append(cache[4]).Append(loader.hour.symbol + ", ").Append(cache[3]).Append(loader.minute.symbol);
            if (includeSeconds)
                stringBuilder.Append(", ").Append(cache[2]).Append(loader.second.symbol);
            return stringBuilder.ToStringAndRelease();
        }
        public String PrintDateNew(Double time, Boolean includeTime)
        {
            String text = CheckNum(time);
            if (text != null)
                return text;

            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            GetDate(time);
            stringBuilder.Append(loader.year.singular + " ").Append(cache[1] + 1).Append(", " + loader.day.singular + " ").Append(cache[5] + 1);
            if (includeTime)
                stringBuilder.AppendFormat(" - {0:D2}:{1:D2}:{2:D2}", cache[4], cache[3], cache[2]);
            return stringBuilder.ToStringAndRelease();
        }

        public virtual String PrintDateCompact(Double time, Boolean includeTime, Boolean includeSeconds = false)
        {
            String text = CheckNum(time);
            if (text != null)
                return text;

            StringBuilder stringBuilder = StringBuilderCache.Acquire();
            GetDate(time);
            stringBuilder.AppendFormat(loader.year.symbol + "{0}, " + loader.day.symbol + "{1:00}", cache[1] + 1, cache[5] + 1);
            if (includeTime)
                stringBuilder.AppendFormat(", {0}:{1:00}", cache[4], cache[3]);
            if (includeSeconds)
                stringBuilder.AppendFormat(":{0:00}", cache[2]);
            return stringBuilder.ToStringAndRelease();
        }

        private static String CheckNum(Double time)
        {
            if (Double.IsNaN(time))
                return "NaN";

            if (Double.IsPositiveInfinity(time))
                return "+Inf";

            if (Double.IsNegativeInfinity(time))
                return "-Inf";

            return null;
        }

        public virtual void GetDate(Double time)
        {
            // This will work also when a year cannot be divided in days without a remainder
            // If the year ends halfway through a day, the clock will go:
            // Year 1 Day 365   ==>   Year 2 Day 0    (Instead of starting directly with Day 1)
            // Day 0 will last untill Day 365 would have ended, then Day 1 will start.
            // This way the time shown by the clock will always be consistent with the position of the sun in the sky

            // Current Year
            Int32 year = (Int32)(time / loader.year.value);

            // Current Day
            Int32 day = (Int32)((time / loader.day.value) - Math.Round(year * loader.year.value / loader.day.value, 0, MidpointRounding.AwayFromZero));

            // Time left to count
            Double left = time % loader.day.value;

            // Number of hours in this day
            Int32 hours = (Int32)(left / loader.hour.value);

            // Time left to count
            left = left - hours * loader.hour.value;

            // Number of minutes in this hour
            Int32 minutes = (Int32)(left / loader.minute.value);

            // Time left to count
            left = left - minutes * loader.minute.value;

            // Number of seconds in this minute
            Int32 seconds = (Int32)(left / loader.second.value);

            cache = new [] { 0, year, seconds, minutes, num4, day };
        }

        public virtual void GetTime(Double time)
        {
            // This will count the number of Years, Days, Hours, Minutes and Seconds
            // If a Year lasts 10.5 days, and time = 14 days, the result will be: 
            // 1 Year, 3 days, and whatever hours-minutes-seconds fit in 0.5 dayloader.second.
            // ( 10.5 + 3 + 0.5 = 14 )

            // Number of years
            Int32 years = (Int32)(time / loader.year.value);

            // Time left to count
            Double left = time - years * loader.year.value;

            // Number of days
            Int32 days = (Int32)(left / loader.day.value);

            // Time left to count
            left = left - days * loader.day.value;

            // Number of hours
            Int32 hours = (Int32)(left / loader.hour.value);

            // Time left to count
            left = left - hours * loader.hour.value;

            // Number of minutes
            Int32 minutes = (Int32)(left / loader.minute.value);

            // Time left to count
            left = left - minutes * loader.minute.value;

            // Number of seconds
            Int32 seconds = (Int32)(left / loader.second.value);

            cache = new[] { 0, years, seconds, minutes, hours, days };
        }

        public virtual Int32 Second
        {
            get { return (Int32)loader.second.value; }
        }
        public virtual Int32 Minute
        {
            get { return (Int32)loader.minute.value; }
        }
        public virtual Int32 Hour
        {
            get { return (Int32)loader.hour.value; }
        }
        public virtual Int32 Day
        {
            get { return (Int32)loader.day.value; }
        }
        public virtual Int32 Year
        {
            get { return (Int32)loader.year.value; }
        }
    }
}
