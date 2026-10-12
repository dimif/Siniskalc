namespace Siniskalc.Views
{
    using System;
    using System.Windows.Controls;
    using Siniskalc.Core.Data;
    using Siniskalc.Data;
    using Siniskalc.ViewModels;

    /// <summary>
    /// Page that hosts the <see cref="HexEditor"/>. It knows how to show a hero; the editor itself works on any
    /// <see cref="IByteEditTarget"/>.
    /// </summary>
    public partial class HexPage : Page
    {
        public HexPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Shows the raw bytes of the hero. Binding again replaces the earlier hero.
        /// </summary>
        public void BindHero(HeroViewModel heroViewModel)
        {
            this.Bind(heroViewModel.CreateByteEditTarget());
        }

        /// <summary>
        /// Shows any editable bytes, e.g. a player or a town. The page takes ownership of the target and disposes it
        /// when another one is bound.
        /// </summary>
        public void Bind(IByteEditTarget target)
        {
            this.editor.Bind(target);
        }
    }
}
