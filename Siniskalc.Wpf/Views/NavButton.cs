namespace Siniskalc.Wpf.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Siniskalc.View"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Siniskalc.View;assembly=Siniskalc.View"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:NavButton/>
    ///
    /// </summary>
    public class NavButton : ListBoxItem
    {
        /// <summary>
        /// Identifies the <see cref="Text"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(NavButton), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="Navlink"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NavlinkProperty = DependencyProperty.Register("Navlink", typeof(Uri), typeof(NavButton), new PropertyMetadata(null));

        /// <summary>
        /// Marks a tab that is only there while the data context of the window has <c>IsAdvancedMode</c> set. It is hidden until the
        /// button is in a window, where its visibility follows that property.
        /// </summary>
        public static readonly DependencyProperty RequiresAdvancedModeProperty = DependencyProperty.Register(
            nameof(RequiresAdvancedMode),
            typeof(bool),
            typeof(NavButton),
            new PropertyMetadata(false, OnRequiresAdvancedModeChanged));

        static NavButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavButton), new FrameworkPropertyMetadata(typeof(NavButton)));
            VisibilityProperty.OverrideMetadata(typeof(NavButton), new FrameworkPropertyMetadata(Visibility.Visible, OnVisibilityChanged));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavButton"/> class.
        /// </summary>
        public NavButton()
        {
            this.Loaded += this.OnLoaded;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the button is shown only while the advanced mode is on.
        /// </summary>
        public bool RequiresAdvancedMode
        {
            get { return (bool)this.GetValue(RequiresAdvancedModeProperty); }
            set { this.SetValue(RequiresAdvancedModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the address of the page the button opens.
        /// </summary>
        public Uri Navlink
        {
            get { return (Uri)this.GetValue(NavlinkProperty); }
            set { this.SetValue(NavlinkProperty, value); }
        }

        /// <summary>
        /// Gets or sets the text of the button.
        /// </summary>
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        private static void OnRequiresAdvancedModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                ((NavButton)d).Visibility = Visibility.Collapsed;
            }
        }

        // A tab that gets hidden while it is the picked one would leave the page of a tab nobody can see: go back to the first tab.
        private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((Visibility)e.NewValue != Visibility.Visible)
            {
                ((NavButton)d).SelectFirstTabWhenSelected();
            }
        }

        // The binding is made once the button is in a window: a binding to an ancestor made before that reports an error.
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!this.RequiresAdvancedMode || Window.GetWindow(this) is not { } window)
            {
                return;
            }

            this.SetBinding(VisibilityProperty, new Binding("DataContext.IsAdvancedMode") { Source = window, Converter = new BooleanToVisibilityConverter() });
            if (this.Visibility != Visibility.Visible)
            {
                this.SelectFirstTabWhenSelected();
            }
        }

        private void SelectFirstTabWhenSelected()
        {
            if (this.IsSelected && ItemsControl.ItemsControlFromItemContainer(this) is ListBox list)
            {
                list.SelectedIndex = 0;
            }
        }
    }
}
