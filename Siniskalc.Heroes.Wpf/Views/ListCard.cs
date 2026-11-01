namespace Siniskalc.Heroes.Wpf.Views
{
    using System.Collections;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// The common ancestor of the cards that list the places of the map (mines, dwellings, obelisks): a card of double width with a heading, a
    /// note, the headings of the columns (its content) and a row for each item. The headings and the rows are laid out by the same inset, so a
    /// heading is always over the cells of its column; a game gives the row template and the headings, with the same column widths.
    /// </summary>
    public class ListCard : ContentControl
    {
        /// <summary>
        /// Identifies the <see cref="Heading"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(nameof(Heading), typeof(string), typeof(ListCard), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the <see cref="Summary"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SummaryProperty = DependencyProperty.Register(nameof(Summary), typeof(string), typeof(ListCard), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the <see cref="Note"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(nameof(Note), typeof(string), typeof(ListCard), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the <see cref="CardWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CardWidthProperty = DependencyProperty.Register(nameof(CardWidth), typeof(double), typeof(ListCard), new PropertyMetadata(672.0));

        /// <summary>
        /// Identifies the <see cref="ListMaxHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ListMaxHeightProperty = DependencyProperty.Register(nameof(ListMaxHeight), typeof(double), typeof(ListCard), new PropertyMetadata(double.PositiveInfinity));

        /// <summary>
        /// Identifies the <see cref="Toolbar"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ToolbarProperty = DependencyProperty.Register(nameof(Toolbar), typeof(object), typeof(ListCard));

        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ListCard));

        /// <summary>
        /// Identifies the <see cref="ItemTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(ListCard));

        /// <summary>
        /// Identifies the <see cref="SelectedItem"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem), typeof(object), typeof(ListCard), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Gets or sets the heading of the card.
        /// </summary>
        public string Heading
        {
            get => (string)this.GetValue(HeadingProperty);
            set => this.SetValue(HeadingProperty, value);
        }

        /// <summary>
        /// Gets or sets the text on the right of the heading, such as "2 of 7 visited".
        /// </summary>
        public string Summary
        {
            get => (string)this.GetValue(SummaryProperty);
            set => this.SetValue(SummaryProperty, value);
        }

        /// <summary>
        /// Gets or sets the note under the heading.
        /// </summary>
        public string Note
        {
            get => (string)this.GetValue(NoteProperty);
            set => this.SetValue(NoteProperty, value);
        }

        /// <summary>
        /// Gets or sets the width of the card. A list is double width by default.
        /// </summary>
        public double CardWidth
        {
            get => (double)this.GetValue(CardWidthProperty);
            set => this.SetValue(CardWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the most height the rows can take before they scroll inside the card. A list of many rows (the dwellings of a map) sets it,
        /// so what is below the card stays in reach; by default the card grows with its rows.
        /// </summary>
        public double ListMaxHeight
        {
            get => (double)this.GetValue(ListMaxHeightProperty);
            set => this.SetValue(ListMaxHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets what is shown between the note and the headings of the columns, such as buttons that act on every row.
        /// </summary>
        public object? Toolbar
        {
            get => this.GetValue(ToolbarProperty);
            set => this.SetValue(ToolbarProperty, value);
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
        /// Gets or sets the row that is picked.
        /// </summary>
        public object? SelectedItem
        {
            get => this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.GetTemplateChild("PART_List") is ListBox list)
            {
                // A row picked from elsewhere (in the tree, say) is brought into view when the rows scroll.
                list.SelectionChanged += (_, _) =>
                {
                    if (list.SelectedItem != null)
                    {
                        list.ScrollIntoView(list.SelectedItem);
                    }
                };
            }
        }
    }
}
