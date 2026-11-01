namespace Siniskalc.Heroes.Wpf.Views
{
    using System.Collections;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// A card with a heading and a list, such as the heroes or the towns of a player, and a note that shows while the list is empty.
    /// </summary>
    public class ItemsCard : Control
    {
        /// <summary>
        /// Identifies the <see cref="Heading"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(nameof(Heading), typeof(string), typeof(ItemsCard), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ItemsCard));

        /// <summary>
        /// Identifies the <see cref="ItemTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(ItemsCard));

        /// <summary>
        /// Identifies the <see cref="EmptyText"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EmptyTextProperty = DependencyProperty.Register(nameof(EmptyText), typeof(string), typeof(ItemsCard), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the <see cref="HasItems"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HasItemsProperty = DependencyProperty.Register(nameof(HasItems), typeof(bool), typeof(ItemsCard), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets the heading of the card.
        /// </summary>
        public string Heading
        {
            get => (string)this.GetValue(HeadingProperty);
            set => this.SetValue(HeadingProperty, value);
        }

        /// <summary>
        /// Gets or sets the items the card lists.
        /// </summary>
        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the template that shows an item.
        /// </summary>
        public DataTemplate? ItemTemplate
        {
            get => (DataTemplate?)this.GetValue(ItemTemplateProperty);
            set => this.SetValue(ItemTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets what is told while there is nothing in the list, such as "This player has no heroes".
        /// </summary>
        public string EmptyText
        {
            get => (string)this.GetValue(EmptyTextProperty);
            set => this.SetValue(EmptyTextProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the list has anything in it; the note shows while it has not.
        /// </summary>
        public bool HasItems
        {
            get => (bool)this.GetValue(HasItemsProperty);
            set => this.SetValue(HasItemsProperty, value);
        }
    }
}
