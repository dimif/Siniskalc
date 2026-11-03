namespace Siniskalc.Homm2.Tests.Integration.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Homm2.Views;

    /// <summary>
    /// The real mines card renders the saved guardians as text, not editable army controls.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    [Apartment(ApartmentState.STA)]
    internal sealed class OwnedMinesCardTests
    {
        [TestCase("mine_guardians_00_baseline", "Unguarded", false)]
        [TestCase("mine_guardians_01_earth", "Earth Elemental × 20", false)]
        [TestCase("mine_guardians_02_air", "Air Elemental × 20", false)]
        [TestCase("mine_guardians_03_fire", "Fire Elemental × 20", false)]
        [TestCase("mine_guardians_04_water", "Water Elemental × 20", false)]
        [TestCase("mine_guardians_05_earth_spell_power_10", "Earth Elemental × 40", false)]
        [TestCase("mine_guardians_06_before_orange_capture", "Earth Elemental × 20", false)]
        [TestCase("mine_guardians_07_orange_captures_mine", "Unguarded", false)]
        [TestCase("mine_guardians_01_earth", "Unknown guardian (spell 91, count 17)", true)]
        public void TheMineGuardiansSavesShowTheirGuardianAsReadOnlyTextWithOnlyTheOwnerEditable(string save, string summary, bool unknownGuardian)
        {
            TestApplication.Ensure();
            var game = TestServices.LoadNamed("MineGuardians", save);
            var mine = game.History.Current.Mines().GetModels().Single(m => m.Position == new MapPosition(43, 23));
            if (unknownGuardian)
            {
                using var target = new ByteEditTargetFactory().ForMine(game.History, mine.Index);
                Assert.That(target.Update(bytes => { bytes[2] = 91; bytes[3] = 17; return bytes; }), Is.True);
            }

            var changesBefore = game.History.GetPendingChanges().Count();
            using var page = new PlayerViewModel(game, mine.Owner, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { });
            var card = new OwnedMinesCard { DataContext = page };
            var window = new Window
            {
                Content = card,
                DataContext = new { IsAdvancedMode = false },
                Width = 1000,
                Height = 700,
                ShowInTaskbar = false,
            };
            window.Show();
            try
            {
                Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
                window.UpdateLayout();

                var texts = Descendants<TextBlock>(card).Where(t => t.IsVisible).Select(t => t.Text).ToList();
                Assert.That(texts, Does.Contain("Guard (read-only)").And.Contain(summary));
                Assert.That(Descendants<ComboBox>(card).Where(c => c.IsVisible).Count(), Is.EqualTo(page.Mines.Rows.Count), "only one owner picker per mine");
                Assert.That(Descendants<TextBox>(card).Where(t => t.IsVisible), Is.Empty, "guardians have no editable count");
                Assert.That(game.History.GetPendingChanges().Count(), Is.EqualTo(changesBefore), "displaying the card does not edit the save");
            }
            finally
            {
                window.Close();
            }
        }

        private static IEnumerable<T> Descendants<T>(DependencyObject root)
            where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                {
                    yield return match;
                }

                foreach (var nested in Descendants<T>(child))
                {
                    yield return nested;
                }
            }
        }
    }
}
