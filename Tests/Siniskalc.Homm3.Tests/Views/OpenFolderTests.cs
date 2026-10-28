using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Moq;
using Siniskalc.Core.Runtime;
using Siniskalc.Wpf.ViewModels;
using Siniskalc.Wpf.Views;

namespace Siniskalc.Homm3.Tests.Views
{
    /// <summary>
    /// Opening the folder of the savegame from the toolbar and from the menu of the node of the game.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class OpenFolderTests
    {
        private static readonly string SavePath = Path.Combine("TestData", "Sod", "conflux_00_baseline.GM1");

        [Test]
        public void TheToolbarHasAnOpenFolderButtonRightAfterReloadThatOpensTheFolderOfTheSavegame()
        {
            TestApplication.Ensure();
            var launcher = new Mock<IUrlLauncher>();
            launcher.Setup(l => l.TryOpenFolder(It.IsAny<string>())).Returns(true);
            var editor = TestServices.CreateHomm3Editor();
            var shell = TestServices.CreateShell(editor);
            var window = TestServices.CreateShellWindow(shell, launcher: launcher.Object);
            window.Show();

            try
            {
                Pump();
                var buttons = Descendants<Button>(window).Where(b => b.Command != null && System.Windows.Automation.AutomationProperties.GetName(b).Length > 0).ToList();
                var names = buttons.Select(System.Windows.Automation.AutomationProperties.GetName).ToList();
                Assert.That(names.IndexOf("Open folder"), Is.EqualTo(names.IndexOf("Reload") + 1));

                var button = buttons[names.IndexOf("Open folder")];
                Assert.That(button.IsEnabled, Is.False, "there is no folder before a savegame is open");

                shell.LoadGame(SavePath);
                Pump();
                Assert.That(button.IsEnabled, Is.True);

                button.Command.Execute(null);

                launcher.Verify(l => l.TryOpenFolder(Path.GetDirectoryName(Path.GetFullPath(SavePath))!), Times.Once);
            }
            finally
            {
                window.Close();
            }
        }

        [Test]
        public void OnlyTheNodeOfTheGameHasAMenuWithReloadSaveAndOpenFolder()
        {
            TestApplication.Ensure();
            var editor = TestServices.CreateHomm3Editor();
            var shell = TestServices.CreateShell(editor);
            shell.LoadGame(SavePath);
            var window = TestServices.CreateShellWindow(shell);
            window.Show();

            try
            {
                Pump();
                window.UpdateLayout();
                var tree = Descendants<TreeView>(window).Single();
                var game = (TreeViewItem)tree.ItemContainerGenerator.ContainerFromIndex(0);

                Assert.That(game.ContextMenu, Is.Not.Null);
                Assert.That(game.ContextMenu!.Items.OfType<MenuItem>().Select(m => (m.Header, m.Command)), Is.EqualTo(new object[]
                {
                    ("Reload save", EditorCommands.Reload),
                    (EditorCommands.OpenFolder.Text, EditorCommands.OpenFolder),
                }));

                game.IsExpanded = true;
                window.UpdateLayout();
                var player = (TreeViewItem)game.ItemContainerGenerator.ContainerFromIndex(0);
                Assert.That(player.ContextMenu, Is.Null);
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

        private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
    }
}
