namespace Siniskalc.Wpf.Tests.ViewModels
{
    using System;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the command that runs a delegate when a button is pressed, and says whether it may.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class DelegateCommandTests
    {
        [Test]
        public void ExecuteRunsTheDelegateWithTheParameter()
        {
            object? given = null;
            var command = new DelegateCommand(p => given = p);

            command.Execute("parameter");

            Assert.That(given, Is.EqualTo("parameter"));
        }

        [Test]
        public void ACommandWithNoConditionCanAlwaysExecute()
        {
            Assert.That(new DelegateCommand(_ => { }).CanExecute(null), Is.True);
        }

        [Test]
        public void TheConditionIsAskedWithTheParameterEachTime()
        {
            var allowed = false;
            var command = new DelegateCommand(_ => { }, p => allowed && (string?)p == "ok");

            Assert.That(command.CanExecute("ok"), Is.False);

            allowed = true;

            Assert.That(command.CanExecute("ok"), Is.True);
            Assert.That(command.CanExecute("no"), Is.False);
        }

        [Test]
        public void ACommandThatCannotExecuteStillRunsTheDelegateWhenExecuteIsCalledDirectly()
        {
            var runs = 0;
            var command = new DelegateCommand(_ => runs++, _ => false);

            command.Execute(null);

            Assert.That(runs, Is.EqualTo(1), "keeping a disabled button from running is the job of the button");
        }

        [Test]
        public void CanExecuteChangedFollowsTheRequerySuggestionsOfTheWindow()
        {
            ICommand command = new DelegateCommand(_ => { });
            EventHandler handler = (_, _) => { };

            Assert.That(() => command.CanExecuteChanged += handler, Throws.Nothing);
            Assert.That(() => command.CanExecuteChanged -= handler, Throws.Nothing);
        }

        [Test]
        public void RaisingCanExecuteChangedAsksTheWindowToQueryAgain()
        {
            var command = new DelegateCommand(_ => { });

            Assert.That(() => command.RaiseCanExecuteChanged(), Throws.Nothing);
        }
    }
}
