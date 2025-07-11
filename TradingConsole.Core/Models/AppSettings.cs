// In TradingConsole.Core/Models/AppSettings.cs
using System.Collections.Generic;

namespace TradingConsole.Core.Models
{
    /// <summary>
    /// Represents a set of user-defined trading levels for a specific index.
    /// </summary>
    public class IndexLevels
    {
        public decimal NoTradeUpperBand { get; set; }
        public decimal NoTradeLowerBand { get; set; }
        public decimal SupportLevel { get; set; }
        public decimal ResistanceLevel { get; set; }
        public decimal Threshold { get; set; } // The point/price value for the S/R threshold band.
    }

    /// <summary>
    /// Represents the application's user-configurable settings that will be saved to a file.
    /// </summary>
    public class AppSettings
    {
        public Dictionary<string, int> FreezeQuantities { get; set; }
        public List<string> MonitoredSymbols { get; set; }
        public int ShortEmaLength { get; set; }
        public int LongEmaLength { get; set; }

        public int AtrPeriod { get; set; }
        public int AtrSmaPeriod { get; set; }

        // --- NEW: Dictionary to store custom levels for each index ---
        public Dictionary<string, IndexLevels> CustomIndexLevels { get; set; }

        public AppSettings()
        {
            FreezeQuantities = new Dictionary<string, int>
            {
                { "NIFTY", 1800 },
                { "BANKNIFTY", 900 },
                { "FINNIFTY", 1800 },
                { "SENSEX", 1000 }
            };

            MonitoredSymbols = new List<string>
            {
                "IDX:Nifty 50",
                "IDX:Nifty Bank",
                "IDX:Sensex",
                "EQ:HDFCBANK",
                "EQ:ICICIBANK",
                "EQ:RELIANCE INDUSTRIES",
                "EQ:INFOSYS",
                "EQ:ITC",
                "EQ:TATA CONSULTANCY",
                "FUT:NIFTY",
                "FUT:BANKNIFTY",
                "FUT:HDFCBANK",
                "FUT:ICICIBANK",
                "FUT:RELIANCE",
                "FUT:INFY",
                "FUT:TCS"
            };

            ShortEmaLength = 9;
            LongEmaLength = 21;

            AtrPeriod = 14;
            AtrSmaPeriod = 10;

            // --- NEW: Initialize with default placeholder levels ---
            CustomIndexLevels = new Dictionary<string, IndexLevels>
            {
                {
                    "NIFTY", new IndexLevels {
                        NoTradeUpperBand = 23500, NoTradeLowerBand = 23400,
                        SupportLevel = 23300, ResistanceLevel = 23600, Threshold = 20
                    }
                },
                {
                    "BANKNIFTY", new IndexLevels {
                        NoTradeUpperBand = 50000, NoTradeLowerBand = 49800,
                        SupportLevel = 49500, ResistanceLevel = 50500, Threshold = 50
                    }
                },
                {
                    "SENSEX", new IndexLevels {
                        NoTradeUpperBand = 77000, NoTradeLowerBand = 76800,
                        SupportLevel = 76500, ResistanceLevel = 77500, Threshold = 100
                    }
                }
            };
        }
    }
}
