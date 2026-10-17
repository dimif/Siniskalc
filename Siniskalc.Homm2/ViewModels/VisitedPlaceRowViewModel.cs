namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Numerics;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One kind of place a hero remembers having visited (forts, gazebos...): how many, and a command that makes the hero forget them so
    /// that it can visit them again.
    /// </summary>
    public sealed class VisitedPlaceRowViewModel : AbstractViewModelBase
    {
        private readonly Func<uint> get;

        public VisitedPlaceRowViewModel(string name, Func<uint> get, Action forget, string toolTip)
        {
            this.Name = name;
            this.get = get;
            this.ToolTip = toolTip;
            this.ForgetCommand = new DelegateCommand(_ => forget(), _ => this.get() != 0);
        }

        public string Name { get; }

        public string ToolTip { get; }

        /// <summary>
        /// Gets how many places of the kind the hero has visited.
        /// </summary>
        public string Text => this.get() switch
        {
            0 => "none",
            var bits => BitOperations.PopCount(bits).ToString(),
        };

        public ICommand ForgetCommand { get; }

        public void Refresh()
        {
            this.RaisePropertyChanged(nameof(this.Text));
            (this.ForgetCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }
    }
}
