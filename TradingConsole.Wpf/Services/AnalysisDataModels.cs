// In TradingConsole.Wpf/Services/AnalysisDataModels.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingConsole.Wpf.Services
{
    public class Candle
    {
        public DateTime Timestamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public long OpenInterest { get; set; }

        public decimal Vwap { get; set; }
        internal decimal CumulativePriceVolume { get; set; } = 0;
        internal long CumulativeVolume { get; set; } = 0;


        public override string ToString()
        {
            return $"T: {Timestamp:HH:mm:ss}, O: {Open}, H: {High}, L: {Low}, C: {Close}, V: {Volume}";
        }
    }

    public class EmaState
    {
        public decimal CurrentShortEma { get; set; }
        public decimal CurrentLongEma { get; set; }
    }

    public class RsiState
    {
        public decimal AvgGain { get; set; }
        public decimal AvgLoss { get; set; }
        public List<decimal> RsiValues { get; } = new List<decimal>();
    }

    public class AtrState
    {
        public decimal CurrentAtr { get; set; }
        public List<decimal> AtrValues { get; } = new List<decimal>();
    }

    public class ObvState
    {
        public decimal CurrentObv { get; set; }
        public List<decimal> ObvValues { get; } = new List<decimal>();
        // --- NEW: Property to store the OBV's moving average ---
        public decimal CurrentMovingAverage { get; set; }
    }

    public class IntradayIvState
    {
        public decimal DayHighIv { get; set; } = 0;
        public decimal DayLowIv { get; set; } = decimal.MaxValue;
        public List<decimal> IvPercentileHistory { get; } = new List<decimal>();

        internal enum PriceZone { Inside, Above, Below }
        internal class CustomLevelState
        {
            public int BreakoutCount { get; set; }
            public int BreakdownCount { get; set; }
            public PriceZone LastZone { get; set; } = PriceZone.Inside;
        }
    }
}
