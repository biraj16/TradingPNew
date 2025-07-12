// In TradingConsole.Wpf/ViewModels/AnalysisTabViewModel.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TradingConsole.Wpf.ViewModels
{
    public class AnalysisTabViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AnalysisResult> AnalysisResults { get; } = new ObservableCollection<AnalysisResult>();

        public AnalysisTabViewModel()
        {
            // Constructor remains parameterless.
        }

        public void UpdateAnalysisResult(AnalysisResult newResult)
        {
            var existingResult = AnalysisResults.FirstOrDefault(r => r.SecurityId == newResult.SecurityId);

            if (existingResult != null)
            {
                existingResult.Symbol = newResult.Symbol;
                existingResult.Vwap = newResult.Vwap;
                existingResult.CurrentVolume = newResult.CurrentVolume;
                existingResult.AvgVolume = newResult.AvgVolume;
                existingResult.VolumeSignal = newResult.VolumeSignal;
                existingResult.OiSignal = newResult.OiSignal;
                existingResult.InstrumentGroup = newResult.InstrumentGroup;
                existingResult.UnderlyingGroup = newResult.UnderlyingGroup;

                existingResult.EmaSignal1Min = newResult.EmaSignal1Min;
                existingResult.EmaSignal5Min = newResult.EmaSignal5Min;
                existingResult.EmaSignal15Min = newResult.EmaSignal15Min;

                existingResult.VwapEmaSignal1Min = newResult.VwapEmaSignal1Min;
                existingResult.VwapEmaSignal5Min = newResult.VwapEmaSignal5Min;
                existingResult.VwapEmaSignal15Min = newResult.VwapEmaSignal15Min;

                existingResult.PriceVsVwapSignal = newResult.PriceVsVwapSignal;
                existingResult.PriceVsCloseSignal = newResult.PriceVsCloseSignal;
                existingResult.DayRangeSignal = newResult.DayRangeSignal;
                existingResult.OpenDriveSignal = newResult.OpenDriveSignal;

                existingResult.CustomLevelSignal = newResult.CustomLevelSignal;
                existingResult.CandleSignal1Min = newResult.CandleSignal1Min;
                existingResult.CandleSignal5Min = newResult.CandleSignal5Min;

                existingResult.CurrentIv = newResult.CurrentIv;
                existingResult.AvgIv = newResult.AvgIv;
                existingResult.IvSignal = newResult.IvSignal;
                existingResult.IvRank = newResult.IvRank;
                existingResult.IvPercentile = newResult.IvPercentile;
                existingResult.IvTrendSignal = newResult.IvTrendSignal;

                existingResult.RsiValue1Min = newResult.RsiValue1Min;
                existingResult.RsiSignal1Min = newResult.RsiSignal1Min;
                existingResult.RsiValue5Min = newResult.RsiValue5Min;
                existingResult.RsiSignal5Min = newResult.RsiSignal5Min;

                existingResult.Atr1Min = newResult.Atr1Min;
                existingResult.AtrSignal1Min = newResult.AtrSignal1Min;
                existingResult.Atr5Min = newResult.Atr5Min;
                existingResult.AtrSignal5Min = newResult.AtrSignal5Min;

                // --- NEW: Update Volume Profile properties ---
                existingResult.Poc = newResult.Poc;
                existingResult.Vah = newResult.Vah;
                existingResult.Val = newResult.Val;
                existingResult.Vpoc = newResult.Vpoc;
                existingResult.MarketProfileSignal = newResult.MarketProfileSignal;
            }
            else
            {
                AnalysisResults.Add(newResult);
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}