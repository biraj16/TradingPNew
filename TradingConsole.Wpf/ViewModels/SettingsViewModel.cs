// In TradingConsole.Wpf/ViewModels/SettingsViewModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TradingConsole.Core.Models;
using TradingConsole.Wpf.Services;

namespace TradingConsole.Wpf.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService;
        private AppSettings _settings;

        public ReadOnlyCollection<string> MonitoredSymbols => new ReadOnlyCollection<string>(_settings.MonitoredSymbols);

        #region Freeze Quantities
        private int _niftyFreezeQuantity;
        public int NiftyFreezeQuantity { get => _niftyFreezeQuantity; set { _niftyFreezeQuantity = value; OnPropertyChanged(); } }
        private int _bankNiftyFreezeQuantity;
        public int BankNiftyFreezeQuantity { get => _bankNiftyFreezeQuantity; set { _bankNiftyFreezeQuantity = value; OnPropertyChanged(); } }
        private int _finNiftyFreezeQuantity;
        public int FinNiftyFreezeQuantity { get => _finNiftyFreezeQuantity; set { _finNiftyFreezeQuantity = value; OnPropertyChanged(); } }
        private int _sensexFreezeQuantity;
        public int SensexFreezeQuantity { get => _sensexFreezeQuantity; set { _sensexFreezeQuantity = value; OnPropertyChanged(); } }
        #endregion

        #region EMA Lengths
        private int _shortEmaLength;
        public int ShortEmaLength { get => _shortEmaLength; set { if (_shortEmaLength != value) { _shortEmaLength = value; OnPropertyChanged(); } } }
        private int _longEmaLength;
        public int LongEmaLength { get => _longEmaLength; set { if (_longEmaLength != value) { _longEmaLength = value; OnPropertyChanged(); } } }
        #endregion

        #region NEW: Volatility (ATR) Settings
        private int _atrPeriod;
        public int AtrPeriod { get => _atrPeriod; set { if (_atrPeriod != value) { _atrPeriod = value; OnPropertyChanged(); } } }
        private int _atrSmaPeriod;
        public int AtrSmaPeriod { get => _atrSmaPeriod; set { if (_atrSmaPeriod != value) { _atrSmaPeriod = value; OnPropertyChanged(); } } }
        #endregion

        #region Custom Index Levels
        // ... (existing index level properties are unchanged)
        private decimal _niftyNoTradeUpper;
        public decimal NiftyNoTradeUpper { get => _niftyNoTradeUpper; set { _niftyNoTradeUpper = value; OnPropertyChanged(); } }
        private decimal _niftyNoTradeLower;
        public decimal NiftyNoTradeLower { get => _niftyNoTradeLower; set { _niftyNoTradeLower = value; OnPropertyChanged(); } }
        private decimal _niftySupport;
        public decimal NiftySupport { get => _niftySupport; set { _niftySupport = value; OnPropertyChanged(); } }
        private decimal _niftyResistance;
        public decimal NiftyResistance { get => _niftyResistance; set { _niftyResistance = value; OnPropertyChanged(); } }
        private decimal _niftyThreshold;
        public decimal NiftyThreshold { get => _niftyThreshold; set { _niftyThreshold = value; OnPropertyChanged(); } }

        private decimal _bankniftyNoTradeUpper;
        public decimal BankniftyNoTradeUpper { get => _bankniftyNoTradeUpper; set { _bankniftyNoTradeUpper = value; OnPropertyChanged(); } }
        private decimal _bankniftyNoTradeLower;
        public decimal BankniftyNoTradeLower { get => _bankniftyNoTradeLower; set { _bankniftyNoTradeLower = value; OnPropertyChanged(); } }
        private decimal _bankniftySupport;
        public decimal BankniftySupport { get => _bankniftySupport; set { _bankniftySupport = value; OnPropertyChanged(); } }
        private decimal _bankniftyResistance;
        public decimal BankniftyResistance { get => _bankniftyResistance; set { _bankniftyResistance = value; OnPropertyChanged(); } }
        private decimal _bankniftyThreshold;
        public decimal BankniftyThreshold { get => _bankniftyThreshold; set { _bankniftyThreshold = value; OnPropertyChanged(); } }

        private decimal _sensexNoTradeUpper;
        public decimal SensexNoTradeUpper { get => _sensexNoTradeUpper; set { _sensexNoTradeUpper = value; OnPropertyChanged(); } }
        private decimal _sensexNoTradeLower;
        public decimal SensexNoTradeLower { get => _sensexNoTradeLower; set { _sensexNoTradeLower = value; OnPropertyChanged(); } }
        private decimal _sensexSupport;
        public decimal SensexSupport { get => _sensexSupport; set { _sensexSupport = value; OnPropertyChanged(); } }
        private decimal _sensexResistance;
        public decimal SensexResistance { get => _sensexResistance; set { _sensexResistance = value; OnPropertyChanged(); } }
        private decimal _sensexThreshold;
        public decimal SensexThreshold { get => _sensexThreshold; set { _sensexThreshold = value; OnPropertyChanged(); } }
        #endregion

        public ICommand SaveSettingsCommand { get; }
        public event EventHandler? SettingsSaved;

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _settings = _settingsService.LoadSettings();
            LoadSettingsIntoViewModel();

            SaveSettingsCommand = new RelayCommand(ExecuteSaveSettings);
        }

        private void LoadSettingsIntoViewModel()
        {
            NiftyFreezeQuantity = _settings.FreezeQuantities.GetValueOrDefault("NIFTY", 1800);
            BankNiftyFreezeQuantity = _settings.FreezeQuantities.GetValueOrDefault("BANKNIFTY", 900);
            FinNiftyFreezeQuantity = _settings.FreezeQuantities.GetValueOrDefault("FINNIFTY", 1800);
            SensexFreezeQuantity = _settings.FreezeQuantities.GetValueOrDefault("SENSEX", 1000);

            ShortEmaLength = _settings.ShortEmaLength;
            LongEmaLength = _settings.LongEmaLength;

            // --- NEW: Load ATR settings ---
            AtrPeriod = _settings.AtrPeriod;
            AtrSmaPeriod = _settings.AtrSmaPeriod;

            var niftyLevels = _settings.CustomIndexLevels.GetValueOrDefault("NIFTY", new IndexLevels());
            NiftyNoTradeUpper = niftyLevels.NoTradeUpperBand;
            NiftyNoTradeLower = niftyLevels.NoTradeLowerBand;
            NiftySupport = niftyLevels.SupportLevel;
            NiftyResistance = niftyLevels.ResistanceLevel;
            NiftyThreshold = niftyLevels.Threshold;

            var bankniftyLevels = _settings.CustomIndexLevels.GetValueOrDefault("BANKNIFTY", new IndexLevels());
            BankniftyNoTradeUpper = bankniftyLevels.NoTradeUpperBand;
            BankniftyNoTradeLower = bankniftyLevels.NoTradeLowerBand;
            BankniftySupport = bankniftyLevels.SupportLevel;
            BankniftyResistance = bankniftyLevels.ResistanceLevel;
            BankniftyThreshold = bankniftyLevels.Threshold;

            var sensexLevels = _settings.CustomIndexLevels.GetValueOrDefault("SENSEX", new IndexLevels());
            SensexNoTradeUpper = sensexLevels.NoTradeUpperBand;
            SensexNoTradeLower = sensexLevels.NoTradeLowerBand;
            SensexSupport = sensexLevels.SupportLevel;
            SensexResistance = sensexLevels.ResistanceLevel;
            SensexThreshold = sensexLevels.Threshold;
        }

        private void ExecuteSaveSettings(object? parameter)
        {
            _settings.FreezeQuantities["NIFTY"] = NiftyFreezeQuantity;
            _settings.FreezeQuantities["BANKNIFTY"] = BankNiftyFreezeQuantity;
            _settings.FreezeQuantities["FINNIFTY"] = FinNiftyFreezeQuantity;
            _settings.FreezeQuantities["SENSEX"] = SensexFreezeQuantity;

            _settings.ShortEmaLength = ShortEmaLength;
            _settings.LongEmaLength = LongEmaLength;

            // --- NEW: Save ATR settings ---
            _settings.AtrPeriod = AtrPeriod;
            _settings.AtrSmaPeriod = AtrSmaPeriod;

            _settings.CustomIndexLevels["NIFTY"] = new IndexLevels
            {
                NoTradeUpperBand = NiftyNoTradeUpper,
                NoTradeLowerBand = NiftyNoTradeLower,
                SupportLevel = NiftySupport,
                ResistanceLevel = NiftyResistance,
                Threshold = NiftyThreshold
            };
            _settings.CustomIndexLevels["BANKNIFTY"] = new IndexLevels
            {
                NoTradeUpperBand = BankniftyNoTradeUpper,
                NoTradeLowerBand = BankniftyNoTradeLower,
                SupportLevel = BankniftySupport,
                ResistanceLevel = BankniftyResistance,
                Threshold = BankniftyThreshold
            };
            _settings.CustomIndexLevels["SENSEX"] = new IndexLevels
            {
                NoTradeUpperBand = SensexNoTradeUpper,
                NoTradeLowerBand = SensexNoTradeLower,
                SupportLevel = SensexSupport,
                ResistanceLevel = SensexResistance,
                Threshold = SensexThreshold
            };

            _settingsService.SaveSettings(_settings);
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }

        public IndexLevels? GetLevelsForIndex(string indexSymbol)
        {
            if (string.IsNullOrEmpty(indexSymbol)) return null;

            string key = indexSymbol.ToUpper();
            string settingsKey;

            if (key.Contains("BANK") && key.Contains("NIFTY"))
            {
                settingsKey = "BANKNIFTY";
            }
            else if (key.Contains("NIFTY"))
            {
                settingsKey = "NIFTY";
            }
            else if (key.Contains("SENSEX"))
            {
                settingsKey = "SENSEX";
            }
            else
            {
                return null;
            }

            return _settings.CustomIndexLevels.GetValueOrDefault(settingsKey);
        }


        public void AddMonitoredSymbol(string symbol)
        {
            if (!_settings.MonitoredSymbols.Any(s => s.Equals(symbol, StringComparison.OrdinalIgnoreCase)))
            {
                _settings.MonitoredSymbols.Add(symbol);
                _settingsService.SaveSettings(_settings);
                OnPropertyChanged(nameof(MonitoredSymbols));
                SettingsSaved?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RemoveMonitoredSymbol(string symbol)
        {
            var symbolToRemove = _settings.MonitoredSymbols.FirstOrDefault(s => s.Equals(symbol, StringComparison.OrdinalIgnoreCase));
            if (symbolToRemove != null)
            {
                _settings.MonitoredSymbols.Remove(symbolToRemove);
                _settingsService.SaveSettings(_settings);
                OnPropertyChanged(nameof(MonitoredSymbols));
                SettingsSaved?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ReplaceMonitoredSymbols(List<string> newSymbols)
        {
            _settings.MonitoredSymbols = newSymbols;
            _settingsService.SaveSettings(_settings);
            OnPropertyChanged(nameof(MonitoredSymbols));
            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
