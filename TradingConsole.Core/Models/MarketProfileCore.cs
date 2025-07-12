// In TradingConsole.Core/Models/MarketProfileCore.cs
using System;

namespace TradingConsole.Core.Models
{
    /// <summary>
    /// Represents the key levels derived from volume profile analysis.
    /// </summary>
    public class VolumeProfileInfo
    {
        public decimal VolumePoc { get; set; }
    }

    /// <summary>
    /// Represents the key levels derived from TPO (Time Price Opportunity) analysis.
    /// </summary>
    public class TpoInfo
    {
        public decimal PointOfControl { get; set; }
        public decimal ValueAreaHigh { get; set; }
        public decimal ValueAreaLow { get; set; }
    }

    /// <summary>
    /// A storable, serializable representation of a single day's market profile.
    /// </summary>
    public class MarketProfileData
    {
        public DateTime Date { get; set; }
        public TpoInfo TpoLevelsInfo { get; set; } = new TpoInfo();
        public VolumeProfileInfo VolumeProfileInfo { get; set; } = new VolumeProfileInfo();
    }
}
