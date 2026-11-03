namespace Siniskalc.Wpf.Views
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Follows a selected record in a control's data context, creating its hex target only while the hex card is visible.
    /// The control owns the presenter's lifetime through its loaded and unloaded events; unloading releases the context and target.
    /// </summary>
    /// <typeparam name="TContext">The data context that supplies the selection and byte target.</typeparam>
    public sealed class SelectedRecordHexPresenter<TContext> : IDisposable
        where TContext : class
    {
        private readonly FrameworkElement owner;
        private readonly FrameworkElement hexCard;
        private readonly HexEditor editor;
        private readonly Func<TContext, INotifyPropertyChanged> getNotifications;
        private readonly Func<TContext, object?> getSelectedKey;
        private readonly Func<TContext, IByteEditTarget?> createTarget;
        private TContext? context;
        private INotifyPropertyChanged? notifications;
        private object? boundKey;
        private bool isAttached;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedRecordHexPresenter{TContext}"/> class.
        /// </summary>
        /// <param name="owner">The control whose data context and loaded lifetime are followed.</param>
        /// <param name="hexCard">The element whose effective visibility gates target creation.</param>
        /// <param name="editor">The editor that takes ownership of each created target.</param>
        /// <param name="getNotifications">Gets the selection's change notifications from the context.</param>
        /// <param name="getSelectedKey">Gets a stable, equality-comparable record key, or null when nothing is selected.</param>
        /// <param name="createTarget">Creates the selected record's target, or null when bytes are unavailable.</param>
        public SelectedRecordHexPresenter(
            FrameworkElement owner,
            FrameworkElement hexCard,
            HexEditor editor,
            Func<TContext, INotifyPropertyChanged> getNotifications,
            Func<TContext, object?> getSelectedKey,
            Func<TContext, IByteEditTarget?> createTarget)
        {
            this.owner = owner;
            this.hexCard = hexCard;
            this.editor = editor;
            this.getNotifications = getNotifications;
            this.getSelectedKey = getSelectedKey;
            this.createTarget = createTarget;
            this.owner.Loaded += this.OnLoaded;
            this.owner.Unloaded += this.OnUnloaded;
            if (this.owner.IsLoaded)
            {
                this.Attach();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.owner.Loaded -= this.OnLoaded;
            this.owner.Unloaded -= this.OnUnloaded;
            this.Detach();
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => this.Attach();

        private void OnUnloaded(object sender, RoutedEventArgs e) => this.Detach();

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) => this.ReplaceContext();

        private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e) => this.ShowSelection();

        private void OnSelectionChanged(object? sender, PropertyChangedEventArgs e) => this.ShowSelection();

        private void Attach()
        {
            if (this.isAttached)
            {
                return;
            }

            this.isAttached = true;
            this.owner.DataContextChanged += this.OnDataContextChanged;
            this.hexCard.IsVisibleChanged += this.OnVisibilityChanged;
            this.ReplaceContext();
        }

        private void Detach()
        {
            if (!this.isAttached)
            {
                return;
            }

            this.isAttached = false;
            this.owner.DataContextChanged -= this.OnDataContextChanged;
            this.hexCard.IsVisibleChanged -= this.OnVisibilityChanged;
            this.UnsubscribeFromContext();
            this.Clear();
        }

        private void UnsubscribeFromContext()
        {
            if (this.notifications != null)
            {
                this.notifications.PropertyChanged -= this.OnSelectionChanged;
            }

            this.notifications = null;
            this.context = null;
        }

        private void ReplaceContext()
        {
            this.UnsubscribeFromContext();
            this.Clear();
            this.context = this.owner.DataContext as TContext;
            if (this.context != null)
            {
                this.notifications = this.getNotifications(this.context);
                this.notifications.PropertyChanged += this.OnSelectionChanged;
            }

            this.ShowSelection();
        }

        private void ShowSelection()
        {
            if (!this.hexCard.IsVisible || this.context == null)
            {
                this.Clear();
                return;
            }

            var key = this.getSelectedKey(this.context);
            if (Equals(key, this.boundKey))
            {
                return;
            }

            this.Clear();
            this.boundKey = key;
            if (key != null && this.createTarget(this.context) is { } target)
            {
                this.editor.Bind(target);
            }
        }

        private void Clear()
        {
            this.boundKey = null;
            this.editor.Unbind();
        }
    }
}
