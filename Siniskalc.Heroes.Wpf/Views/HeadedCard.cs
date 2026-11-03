namespace Siniskalc.Heroes.Wpf.Views
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// A card with a heading, a short summary on the right of it, and what the page puts in the card (its content).
    /// </summary>
    public class HeadedCard : ContentControl
    {
        /// <summary>
        /// Identifies the <see cref="Heading"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(nameof(Heading), typeof(string), typeof(HeadedCard), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the <see cref="Summary"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SummaryProperty = DependencyProperty.Register(nameof(Summary), typeof(string), typeof(HeadedCard), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the <see cref="CardWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CardWidthProperty = DependencyProperty.Register(nameof(CardWidth), typeof(double), typeof(HeadedCard), new PropertyMetadata(330.0));

        /// <summary>
        /// Gets or sets the heading of the card.
        /// </summary>
        public string Heading
        {
            get => (string)this.GetValue(HeadingProperty);
            set => this.SetValue(HeadingProperty, value);
        }

        /// <summary>
        /// Gets or sets the text on the right of the heading, such as "3 of 5 built".
        /// </summary>
        public string Summary
        {
            get => (string)this.GetValue(SummaryProperty);
            set => this.SetValue(SummaryProperty, value);
        }

        /// <summary>
        /// Gets or sets the width of the card.
        /// </summary>
        public double CardWidth
        {
            get => (double)this.GetValue(CardWidthProperty);
            set => this.SetValue(CardWidthProperty, value);
        }
    }
}
