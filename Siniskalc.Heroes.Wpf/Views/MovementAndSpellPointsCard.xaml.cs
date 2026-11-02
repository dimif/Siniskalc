namespace Siniskalc.Heroes.Wpf.Views
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// The card of the movement and spell points of a hero. The data context is a <c>MovementAndSpellPointsCardViewModel</c>; what the game puts in
    /// <see cref="ExtraContent"/> shows under the movement (HoMM3 puts the flag "moved today" there).
    /// </summary>
    public partial class MovementAndSpellPointsCard : UserControl
    {
        /// <summary>
        /// Identifies the <see cref="ExtraContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ExtraContentProperty = DependencyProperty.Register(nameof(ExtraContent), typeof(object), typeof(MovementAndSpellPointsCard), new PropertyMetadata(null, OnExtraContentChanged));

        /// <summary>
        /// Initializes a new instance of the <see cref="MovementAndSpellPointsCard"/> class.
        /// </summary>
        public MovementAndSpellPointsCard()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the content a game adds to the card.
        /// </summary>
        public object? ExtraContent
        {
            get => this.GetValue(ExtraContentProperty);
            set => this.SetValue(ExtraContentProperty, value);
        }

        private static void OnExtraContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MovementAndSpellPointsCard)d).extra.Content = e.NewValue;
        }
    }
}
