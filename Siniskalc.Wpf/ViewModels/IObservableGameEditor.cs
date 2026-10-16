namespace Siniskalc.Wpf.ViewModels
{
    using System.ComponentModel;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// An <see cref="IGameEditor"/> that tells the window when something it shows has changed: what the shell binds to.
    /// </summary>
    public interface IObservableGameEditor : IGameEditor, INotifyPropertyChanged
    {
    }
}
