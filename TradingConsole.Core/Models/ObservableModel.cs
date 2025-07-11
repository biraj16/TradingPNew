using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TradingConsole.Core.Models
{
    /// <summary>
    /// Base class for models that need to notify UI of property changes.
    /// Implements INotifyPropertyChanged to enable data binding updates in WPF.
    /// </summary>
    public class ObservableModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event to notify data-bound UI elements of a property change.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed. Automatically populated by CallerMemberName.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
