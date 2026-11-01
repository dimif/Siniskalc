namespace Siniskalc.Wpf.Views
{
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Siniskalc.Core;

    /// <summary>
    /// A number box with a minus and a plus button. Typing a value, or pressing a button, changes <see cref="Value"/>;
    /// typed values and button presses stay between <see cref="Minimum"/> and <see cref="Maximum"/>.
    /// A value set from the view model is shown as it is, even outside that range.
    /// </summary>
    public partial class Stepper : UserControl
    {
        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(long),
            typeof(Stepper),
            new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        /// <summary>
        /// Identifies the <see cref="Minimum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(long), typeof(Stepper), new PropertyMetadata(0L));

        /// <summary>
        /// Identifies the <see cref="Maximum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(long), typeof(Stepper), new PropertyMetadata(99999999L));

        /// <summary>
        /// Identifies the <see cref="Step"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(nameof(Step), typeof(long), typeof(Stepper), new PropertyMetadata(1L));

        /// <summary>
        /// Identifies the <see cref="ShowButtons"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowButtonsProperty = DependencyProperty.Register(
            nameof(ShowButtons), typeof(bool), typeof(Stepper), new PropertyMetadata(true, OnShowButtonsChanged));

        /// <summary>
        /// Initializes a new instance of the <see cref="Stepper"/> class.
        /// </summary>
        public Stepper()
        {
            this.InitializeComponent();
            this.ShowValue();
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public long Value
        {
            get => (long)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Gets or sets the least value the stepper allows.
        /// </summary>
        public long Minimum
        {
            get => (long)this.GetValue(MinimumProperty);
            set => this.SetValue(MinimumProperty, value);
        }

        /// <summary>
        /// Gets or sets the greatest value the stepper allows.
        /// </summary>
        public long Maximum
        {
            get => (long)this.GetValue(MaximumProperty);
            set => this.SetValue(MaximumProperty, value);
        }

        /// <summary>
        /// Gets or sets how much a button press changes the value. Shift multiplies it by ten.
        /// </summary>
        public long Step
        {
            get => (long)this.GetValue(StepProperty);
            set => this.SetValue(StepProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the minus and plus buttons are shown; without them it is a plain number box.
        /// </summary>
        public bool ShowButtons
        {
            get => (bool)this.GetValue(ShowButtonsProperty);
            set => this.SetValue(ShowButtonsProperty, value);
        }

        private static void OnShowButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var stepper = (Stepper)d;
            var visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            stepper.down.Visibility = visibility;
            stepper.up.Visibility = visibility;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Stepper)d).ShowValue();
        }

        private void ShowValue()
        {
            // Typing does not change Value until it is committed, so this never overwrites what the user is typing.
            this.box.Text = this.Value.ToString(CultureInfo.CurrentCulture);
        }

        private void Commit()
        {
            if (long.TryParse(this.box.Text.Trim(), NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var typed))
            {
                this.Value = RangeGuard.Clamp(typed, this.Minimum, this.Maximum);
            }

            // Shows the clamped value, or puts the old one back when the text was not a number.
            this.box.Text = this.Value.ToString(CultureInfo.CurrentCulture);
        }

        private void Nudge(int direction)
        {
            var step = this.Step * (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? 10 : 1);
            this.Value = RangeGuard.Clamp(this.Value + (direction * step), this.Minimum, this.Maximum);
        }

        private void UpClick(object sender, RoutedEventArgs e) => this.Nudge(1);

        private void DownClick(object sender, RoutedEventArgs e) => this.Nudge(-1);

        private void BoxLostFocus(object sender, RoutedEventArgs e) => this.Commit();

        private void BoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => this.box.SelectAll();

        private void BoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    this.Commit();
                    this.box.SelectAll();
                    e.Handled = true;
                    break;
                case Key.Up:
                    this.Commit();
                    this.Nudge(1);
                    e.Handled = true;
                    break;
                case Key.Down:
                    this.Commit();
                    this.Nudge(-1);
                    e.Handled = true;
                    break;
                case Key.Escape:
                    this.box.Text = this.Value.ToString(CultureInfo.CurrentCulture);
                    this.box.SelectAll();
                    e.Handled = true;
                    break;
            }
        }
    }
}
