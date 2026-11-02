namespace Siniskalc.Wpf.ViewModels
{
    using System.ComponentModel;

    public abstract class AbstractViewModelBase : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Tells the bindings that a property changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed; empty for all of them.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Tells every binding to read its property again.
        /// </summary>
        protected void RaiseAllPropertiesChanged()
        {
            this.RaisePropertyChanged(string.Empty);
        }
    }
}
