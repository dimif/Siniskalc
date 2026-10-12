namespace Siniskalc.Wpf.Views
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// A search box: a text box with a placeholder that says what can be searched for.
    /// </summary>
    public partial class SearchBox : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(SearchBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(SearchBox),
            new PropertyMetadata(string.Empty));

        public SearchBox()
        {
            this.InitializeComponent();
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public string Placeholder
        {
            get => (string)this.GetValue(PlaceholderProperty);
            set => this.SetValue(PlaceholderProperty, value);
        }
    }
}
