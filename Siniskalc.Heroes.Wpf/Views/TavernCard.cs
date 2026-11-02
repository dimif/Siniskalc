namespace Siniskalc.Heroes.Wpf.Views
{
    using System.Collections;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// The card of the tavern of a player: a choice of hero for each of its places, and a link to the hero. Its items are
    /// <see cref="Siniskalc.Heroes.Wpf.ViewModels.TavernPlaceViewModel"/>s.
    /// </summary>
    public class TavernCard : Control
    {
        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(TavernCard));

        /// <summary>
        /// Gets or sets the taverns the card lists.
        /// </summary>
        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }
    }
}
