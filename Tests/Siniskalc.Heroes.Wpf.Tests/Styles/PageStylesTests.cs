namespace Siniskalc.Heroes.Wpf.Tests.Styles
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Tests the styles and templates the pages of every Heroes game share.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class PageStylesTests
    {
        private ResourceDictionary styles = null!;

        [OneTimeSetUp]
        public void LoadTheStyles()
        {
            _ = Application.Current ?? new Application();
            this.styles = new ResourceDictionary { Source = new Uri("pack://application:,,,/Siniskalc.Heroes.Wpf;component/Styles/PageStyles.xaml") };
        }

        [TestCase("TabPanelBase")]
        [TestCase("TabPanel0")]
        [TestCase("TabPanel6")]
        [TestCase("EmptyNoteStyle")]
        [TestCase("EditedMarkStyle")]
        [TestCase("ResetPillStyle")]
        [TestCase("FactRowTemplate")]
        [TestCase("LinkTemplate")]
        public void TheStylesAndTemplatesOfThePagesAreThere(string key)
        {
            Assert.That(this.styles.Contains(key), Is.True);
        }

        [Test]
        public void APanelIsShownOnlyWhileItsTabIsTheSelectedOne()
        {
            var tabs = new Tabs { SelectedTab = 2 };

            var shown = Panel("TabPanel2", tabs);
            var hidden = Panel("TabPanel1", tabs);

            Assert.That(shown.Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(hidden.Visibility, Is.EqualTo(Visibility.Collapsed));

            FrameworkElement Panel(string key, Tabs context)
            {
                var panel = new StackPanel { Style = (Style)this.styles[key], DataContext = context };
                var window = new Window { Content = panel };
                window.Show();
                window.Close();
                return panel;
            }
        }

        private sealed class Tabs
        {
            public int SelectedTab { get; init; }
        }
    }
}
