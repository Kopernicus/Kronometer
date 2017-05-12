/**
 * Kronometer - Makes the KSP clock easily editable
 * Copyright (c) 2017 Sigma88, Thomas P.
 * Licensed under the Terms of the MIT License
 */

using System;
using Kopernicus;

namespace Kronometer
{
    /// <summary>
    /// Loads Clock Definitions
    /// </summary>
    [RequireConfigType(ConfigType.Node)]
    public class ClockLoader : IParserEventSubscriber
    {
        // Load custom definition of seconds
        [ParserTarget("Second", allowMerge = true)]
        public ClockFormatLoader second = new ClockFormatLoader("Second", "Seconds", "s", 1);

        // Load custom definition of minute
        [ParserTarget("Minute", allowMerge = true)]
        public ClockFormatLoader minute = new ClockFormatLoader("Minute", "Minutes", "m", 60);

        // Load custom definition of hour
        [ParserTarget("Hour", allowMerge = true)]
        public ClockFormatLoader hour = new ClockFormatLoader("Hour", "Hours", "h", 3600);

        // Load custom definition of day
        [ParserTarget("Day", allowMerge = true)]
        public ClockFormatLoader day = new ClockFormatLoader("Day", "Days", "d", 3600 * (GameSettings.KERBIN_TIME ? 6 : 24));

        // Load custom definition of year
        [ParserTarget("Year", allowMerge = true)]
        public ClockFormatLoader year = new ClockFormatLoader("Year", "Years", "y", 3600 * (GameSettings.KERBIN_TIME ? 6 * 426 : 24 * 365));
        
        // Whether to calculate time based on orbital parameters
        [ParserTarget("useOrbitalParams")]
        public Boolean useOrbitalParams;

        // Parser Apply Event
        void IParserEventSubscriber.Apply(ConfigNode node) { }

        // Parser Post Apply Event
        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
            ClockFormatter.S = second;
            ClockFormatter.M = minute;
            ClockFormatter.H = hour;
            ClockFormatter.D = day;
            ClockFormatter.Y = year;
        }
    }

    [RequireConfigType(ConfigType.Node)]
    public class ClockFormatLoader : IParserEventSubscriber
    {
        [ParserTarget("singular")]
        public String singular;

        [ParserTarget("plural")]
        public String plural;

        [ParserTarget("symbol")]
        public String symbol;

        [ParserTarget("value")]
        public NumericParser<Double> value;

        // Apply Event
        void IParserEventSubscriber.Apply(ConfigNode node) { }

        // Parser Post Apply Event
        void IParserEventSubscriber.PostApply(ConfigNode node) { }

        public ClockFormatLoader(String singular, String plural, String symbol, Double value)
        {
            this.singular = singular;
            this.plural = plural;
            this.symbol = symbol;
            this.value = value;
        }

    }
}
