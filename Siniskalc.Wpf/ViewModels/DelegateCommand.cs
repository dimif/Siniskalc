namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Windows.Input;

    public class DelegateCommand : ICommand
    {
        private readonly Action<object?> execute;
        private readonly Func<object?, bool>? canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        /// <param name="execute">What the command does; it gets the parameter of the command.</param>
        /// <param name="canExecute">Says whether the command can run for a parameter; the command can always run when omitted.</param>
        public DelegateCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        // Buttons ask again whenever WPF sees input or focus change, and when RaiseCanExecuteChanged is called.
        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <inheritdoc/>
        public bool CanExecute(object? parameter) => this.canExecute == null || this.canExecute(parameter);

        /// <inheritdoc/>
        public void Execute(object? parameter) => this.execute(parameter);

        /// <summary>
        /// Asks WPF to ask every command again whether it can run.
        /// </summary>
        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}
