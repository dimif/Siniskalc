namespace Siniskalc.Wpf.Views
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Security;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A search box, drop-down filters and a grouped list of rows. Set its DataContext to a <see cref="AbstractFinderViewModel"/>:
    /// the view model says which columns there are, which filters, and what the button on a row does.
    /// Put extra controls in the search row with <see cref="ToolbarContent"/>; they get the view model as their DataContext.
    /// </summary>
    public partial class FinderView : UserControl
    {
        public static readonly DependencyProperty ToolbarContentProperty =
            DependencyProperty.Register(nameof(ToolbarContent), typeof(object), typeof(FinderView));

        public static readonly DependencyProperty ListMaxHeightProperty =
            DependencyProperty.Register(nameof(ListMaxHeight), typeof(double), typeof(FinderView), new PropertyMetadata(360.0));

        private readonly Style? plainItemStyle;

        public FinderView()
        {
            this.InitializeComponent();
            this.plainItemStyle = this.list.ItemContainerStyle;
            this.DataContextChanged += (_, _) => this.BuildColumns();
        }

        /// <summary>
        /// Gets or sets extra controls shown after the filters.
        /// </summary>
        public object? ToolbarContent
        {
            get => this.GetValue(ToolbarContentProperty);
            set => this.SetValue(ToolbarContentProperty, value);
        }

        /// <summary>
        /// Gets or sets how tall the list may grow before it scrolls. Use infinity to fill the room the page gives it.
        /// </summary>
        public double ListMaxHeight
        {
            get => (double)this.GetValue(ListMaxHeightProperty);
            set => this.SetValue(ListMaxHeightProperty, value);
        }

        private static string WidthText(GridLength width) => width.IsAuto
            ? "Auto"
            : width.IsStar
                ? width.Value.ToString(CultureInfo.InvariantCulture) + "*"
                : width.Value.ToString(CultureInfo.InvariantCulture);

        private static DataTemplate BuildRowTemplate(AbstractFinderViewModel finder)
        {
            var xaml = new StringBuilder();
            xaml.Append("<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>");
            xaml.Append("<Grid");
            if (finder.RowToolTipPath != null)
            {
                xaml.Append(" ToolTip='{Binding ").Append(SecurityElement.Escape(finder.RowToolTipPath)).Append("}'");
            }

            xaml.Append("><Grid.ColumnDefinitions>");
            foreach (var column in finder.Columns)
            {
                xaml.Append("<ColumnDefinition Width='").Append(WidthText(column.Width)).Append("'/>");
            }

            if (finder.HasAction)
            {
                xaml.Append("<ColumnDefinition Width='70'/>");
            }

            if (finder.HasSecondaryAction)
            {
                xaml.Append("<ColumnDefinition Width='84'/>");
            }

            xaml.Append("</Grid.ColumnDefinitions>");

            for (var i = 0; i < finder.Columns.Count; i++)
            {
                var column = finder.Columns[i];
                var path = SecurityElement.Escape(column.Path);
                var name = SecurityElement.Escape(column.Header);
                switch (column.Kind)
                {
                    case FinderColumnKind.CheckBox:
                        xaml.Append($"<CheckBox Grid.Column='{i}' Margin='14,0,0,0' VerticalAlignment='Center' IsChecked='{{Binding {path}}}' AutomationProperties.Name='{name}'/>");
                        break;
                    case FinderColumnKind.ReadOnlyCheckBox:
                        xaml.Append($"<CheckBox Grid.Column='{i}' Margin='14,0,0,0' VerticalAlignment='Center' IsChecked='{{Binding {path}, Mode=OneWay}}' IsHitTestVisible='False' Focusable='False' IsTabStop='False' AutomationProperties.Name='{name}'/>");
                        break;
                    case FinderColumnKind.MutedText:
                        xaml.Append($"<TextBlock Grid.Column='{i}' Margin='0,0,8,0' VerticalAlignment='Center' TextTrimming='CharacterEllipsis' Style='{{DynamicResource MutedTextStyle}}' Text='{{Binding {path}}}'/>");
                        break;
                    default:
                        xaml.Append($"<TextBlock Grid.Column='{i}' Margin='0,0,8,0' VerticalAlignment='Center' TextTrimming='CharacterEllipsis' Text='{{Binding {path}}}'/>");
                        break;
                }
            }

            if (finder.HasAction)
            {
                xaml.Append($"<Button Grid.Column='{finder.Columns.Count}' Style='{{DynamicResource SlotButtonStyle}}' Width='60' HorizontalAlignment='Left' ");
                xaml.Append("Content='{Binding DataContext.ActionLabel, RelativeSource={RelativeSource AncestorType=ListBox}}' ");
                xaml.Append("Command='{Binding DataContext.ChooseCommand, RelativeSource={RelativeSource AncestorType=ListBox}}' ");
                xaml.Append("CommandParameter='{Binding}'/>");
            }

            if (finder.HasSecondaryAction)
            {
                var column = finder.Columns.Count + (finder.HasAction ? 1 : 0);
                xaml.Append($"<Button Grid.Column='{column}' Style='{{DynamicResource SlotButtonStyle}}' Width='76' HorizontalAlignment='Left' ");
                xaml.Append("Content='{Binding DataContext.SecondaryActionLabel, RelativeSource={RelativeSource AncestorType=ListBox}}' ");
                xaml.Append("Command='{Binding DataContext.ChooseSecondaryCommand, RelativeSource={RelativeSource AncestorType=ListBox}}' ");
                xaml.Append("CommandParameter='{Binding}'/>");
            }

            xaml.Append("</Grid></DataTemplate>");
            return (DataTemplate)XamlReader.Parse(xaml.ToString());
        }

        private static void AddMenuItems(ItemCollection target, IReadOnlyList<FinderMenuItem> items)
        {
            foreach (var entry in items)
            {
                var menuItem = new MenuItem { Header = entry.Header };
                if (entry.Invoke != null)
                {
                    var invoke = entry.Invoke;
                    menuItem.Click += (_, _) => invoke();
                }

                if (entry.Children != null)
                {
                    AddMenuItems(menuItem.Items, entry.Children);
                }

                target.Add(menuItem);
            }
        }

        /// <summary>
        /// Builds the heading row and the row template from the columns of the view model.
        /// </summary>
        private void BuildColumns()
        {
            this.header.Children.Clear();
            this.header.ColumnDefinitions.Clear();
            this.list.ItemTemplate = null;

            if (this.DataContext is not AbstractFinderViewModel finder)
            {
                return;
            }

            for (var i = 0; i < finder.Columns.Count; i++)
            {
                var column = finder.Columns[i];
                this.header.ColumnDefinitions.Add(new ColumnDefinition { Width = column.Width });

                var heading = new TextBlock { Text = column.Header, Margin = new Thickness(0), ToolTip = column.ToolTip };
                heading.SetResourceReference(StyleProperty, "CardHeadingStyle");
                Grid.SetColumn(heading, i);
                this.header.Children.Add(heading);
            }

            if (finder.HasAction)
            {
                this.header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            }

            if (finder.HasSecondaryAction)
            {
                this.header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(84) });
            }

            this.list.ItemTemplate = BuildRowTemplate(finder);
            this.list.SelectionMode = finder.AllowsMultiSelect ? SelectionMode.Extended : SelectionMode.Single;
            this.list.ItemContainerStyle = finder.AllowsMultiSelect ? this.CreateSelectableItemStyle() : this.plainItemStyle;
        }

        /// <summary>
        /// A row style that keeps the selection in the row's <c>IsSelected</c> property and lets the row take the keyboard focus.
        /// </summary>
        private Style CreateSelectableItemStyle()
        {
            var style = new Style(typeof(ListBoxItem), this.plainItemStyle);
            style.Setters.Add(new Setter(FocusableProperty, true));
            style.Setters.Add(new Setter(ListBoxItem.IsSelectedProperty, new Binding("IsSelected") { Mode = BindingMode.TwoWay }));
            return style;
        }

        /// <summary>
        /// Builds the right-click menu for the row under the pointer; no menu when the finder has none for it.
        /// </summary>
        private void ListContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            this.list.ContextMenu = null;
            if (this.DataContext is not AbstractFinderViewModel finder)
            {
                e.Handled = true;
                return;
            }

            for (var element = e.OriginalSource as DependencyObject; element != null && element != sender; element = VisualTreeHelper.GetParent(element))
            {
                if (element is ListBoxItem item)
                {
                    var entries = finder.GetRowMenu(item.DataContext);
                    if (entries.Count > 0)
                    {
                        var menu = new ContextMenu();
                        AddMenuItems(menu.Items, entries);
                        this.list.ContextMenu = menu;
                        return;
                    }

                    break;
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Runs the action on the row that was double-clicked.
        /// </summary>
        private void ListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is not AbstractFinderViewModel { HasAction: true } finder)
            {
                return;
            }

            // Only a double-click on a row counts; the button has its own command.
            for (var element = e.OriginalSource as DependencyObject; element != null && element != sender; element = VisualTreeHelper.GetParent(element))
            {
                if (element is Button or CheckBox)
                {
                    return;
                }

                if (element is ListBoxItem item)
                {
                    finder.ChooseRow(item.DataContext);
                    return;
                }
            }
        }
    }
}
