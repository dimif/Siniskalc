namespace Siniskalc.Wpf.ViewModels
{
    using System.ComponentModel;

    public abstract class AbstractViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

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
