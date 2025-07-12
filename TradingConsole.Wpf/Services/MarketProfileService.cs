// In TradingConsole.Wpf/Services/MarketProfileService.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TradingConsole.Core.Models;

namespace TradingConsole.Wpf.Services
{
    /// <summary>
    /// Manages loading and saving historical Market Profile data to a persistent file.
    /// </summary>
    public class MarketProfileService
    {
        private readonly string _filePath;
        private HistoricalMarketProfileDatabase _database;

        public MarketProfileService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolderPath = Path.Combine(appDataPath, "TradingConsole");
            Directory.CreateDirectory(appFolderPath);
            _filePath = Path.Combine(appFolderPath, "historical_market_profile.json");

            _database = LoadDatabase();
        }

        private HistoricalMarketProfileDatabase LoadDatabase()
        {
            if (!File.Exists(_filePath))
            {
                return new HistoricalMarketProfileDatabase();
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                var db = JsonConvert.DeserializeObject<HistoricalMarketProfileDatabase>(json);
                return db ?? new HistoricalMarketProfileDatabase();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MarketProfileService] Error loading profile database: {ex.Message}");
                return new HistoricalMarketProfileDatabase(); // Return a fresh DB if file is corrupt
            }
        }

        public void SaveDatabase()
        {
            try
            {
                // Prune old records before saving to keep the file size manageable.
                PruneOldRecords();
                string json = JsonConvert.SerializeObject(_database, Formatting.Indented);
                File.WriteAllText(_filePath, json);
                Debug.WriteLine("[MarketProfileService] Successfully saved profile database.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MarketProfileService] Error saving profile database: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates or adds the market profile for a given instrument for the current day.
        /// </summary>
        public void UpdateProfile(string securityId, MarketProfileData profileData)
        {
            if (string.IsNullOrEmpty(securityId)) return;

            if (!_database.Records.ContainsKey(securityId))
            {
                _database.Records[securityId] = new List<MarketProfileData>();
            }

            var todayRecord = _database.Records[securityId].FirstOrDefault(r => r.Date.Date == DateTime.Today);

            if (todayRecord != null)
            {
                // Update the existing record for today
                todayRecord.TpoLevelsInfo = profileData.TpoLevelsInfo;
                todayRecord.VolumeProfileInfo = profileData.VolumeProfileInfo;
            }
            else
            {
                // Add a new record for today
                profileData.Date = DateTime.Today;
                _database.Records[securityId].Add(profileData);
            }
        }

        /// <summary>
        /// Retrieves the historical market profiles for a specific instrument.
        /// </summary>
        public List<MarketProfileData> GetHistoricalProfiles(string securityId)
        {
            if (_database.Records.TryGetValue(securityId, out var profiles))
            {
                return profiles.OrderByDescending(p => p.Date).ToList();
            }
            return new List<MarketProfileData>();
        }

        /// <summary>
        /// Removes records older than 30 days to prevent the database file from growing indefinitely.
        /// </summary>
        private void PruneOldRecords()
        {
            var thirtyDaysAgo = DateTime.Today.AddDays(-30);
            foreach (var key in _database.Records.Keys)
            {
                _database.Records[key].RemoveAll(r => r.Date < thirtyDaysAgo);
            }
        }
    }
}
