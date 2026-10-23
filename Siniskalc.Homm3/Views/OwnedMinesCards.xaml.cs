namespace Siniskalc.Homm3.Views
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// Shows the mines and the guard of the mine that is picked of one owner, and the bytes of that mine. All behaviour is in
    /// <see cref="OwnedMapObjectsViewModel"/>; the control only binds to it and shows the bytes of the mine that is picked.
    /// </summary>
    public partial class OwnedMinesCards : UserControl
    {
        private OwnedMapObjectsViewModel? viewModel;
        private int boundMine = -1;

        public OwnedMinesCards()
        {
            this.InitializeComponent();
            AdvancedModeVisibility.Apply(this.hexCard);
            this.DataContextChanged += this.OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.viewModel != null)
            {
                this.viewModel.PropertyChanged -= this.OnViewModelChanged;
            }

            this.viewModel = e.NewValue as OwnedMapObjectsViewModel;
            this.boundMine = -1;
            if (this.viewModel != null)
            {
                this.viewModel.PropertyChanged += this.OnViewModelChanged;
            }

            this.ShowBytes();
        }

        private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e) => this.ShowBytes();

        // The bytes follow the mine that is picked; an edit of the same mine leaves the editor as it is.
        private void ShowBytes()
        {
            var picked = this.viewModel?.Mines.SelectedMine?.Index ?? -1;
            if (picked == this.boundMine)
            {
                return;
            }

            this.boundMine = picked;
            if (this.viewModel?.CreateMineByteEditTarget() is { } target)
            {
                this.hexEditor.Bind(target);
            }
            else
            {
                this.hexEditor.Unbind();
            }
        }
    }
}
