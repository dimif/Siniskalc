namespace Siniskalc.Wpf.Views
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Microsoft.Win32;
    using Siniskalc.Core.Data;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A hex and ASCII editor for any <see cref="IByteEditTarget"/>. It colours the bytes of known regions, marks
    /// bytes that differ from the loaded save, and lists the regions so they can be selected. Edits are written
    /// through the target, so each one is a single step in the savefile's undo history. The grid is updated in place
    /// when the bytes change elsewhere (another page, undo, redo), so the selection and scroll position stay.
    /// </summary>
    public partial class HexEditor : UserControl
    {
        /// <summary>
        /// The path of the savefile that is open. Set it once on the window: it is inherited by every hex editor inside, which uses
        /// its name to suggest a name when the bytes are exported.
        /// </summary>
        public static readonly DependencyProperty SourceFileProperty = DependencyProperty.RegisterAttached(
            "SourceFile",
            typeof(string),
            typeof(HexEditor),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        private const int BytesPerRow = 16;
        private const int AddressColumnIndex = 0;
        private const int FirstByteColumnIndex = 1;
        private const int LastByteColumnIndex = 16;
        private const int AsciiColumnIndex = 17;

        private IByteEditTarget? target;
        private byte[] bytes = [];
        private byte[] original = [];
        private ByteRegionMap map = new([], 0);
        private DataTable? table;
        private bool isRebuilding;
        private bool isEditingCell;
        private bool isNormalizingSelection;
        private int selectionAnchor = -1;
        private bool hasPendingSync;

        public HexEditor()
        {
            this.InitializeComponent();
            this.BuildGridColumns();

            // Edits made while the control was hidden may have been missed, so show them when it comes back.
            this.IsVisibleChanged += (_, e) =>
            {
                if (this.target != null && (bool)e.NewValue)
                {
                    this.SyncFromTarget();
                }
            };

            this.Unloaded += (_, _) => this.UnsubscribeFromTarget();
            this.Loaded += (_, _) =>
            {
                this.SubscribeToTarget();
                this.SyncFromTarget();
            };
        }

        public static string? GetSourceFile(DependencyObject element) => (string?)element.GetValue(SourceFileProperty);

        public static void SetSourceFile(DependencyObject element, string? value) => element.SetValue(SourceFileProperty, value);

        /// <summary>
        /// Shows <paramref name="newTarget"/> and takes ownership of it: the target is disposed when another one is
        /// bound or <see cref="Unbind"/> is called. Any target bound before is disposed.
        /// </summary>
        public void Bind(IByteEditTarget newTarget)
        {
            this.Unbind();
            this.target = newTarget;
            this.SubscribeToTarget();
            this.RebuildGrid();
        }

        /// <summary>
        /// Stops showing the current target and disposes it.
        /// </summary>
        public void Unbind()
        {
            this.UnsubscribeFromTarget();
            this.target?.Dispose();
            this.target = null;
            this.hasPendingSync = false;
            this.isEditingCell = false;
            this.bytes = [];
            this.original = [];
            this.map = new ByteRegionMap([], 0);
            this.bytesGrid.ItemsSource = null;
            this.regionList.ItemsSource = null;
            this.legend.ItemsSource = null;
            this.titleText.Text = "-";
            this.changedText.Text = "No bytes changed";
            this.UpdateStatusBar();
        }

        private static string GetByteColumnKey(int offset) => $"B{offset:X2}";

        private static string GetRegionColumnKey(int offset) => $"K{offset:X2}";

        private static string GetChangedColumnKey(int offset) => $"D{offset:X2}";

        private static string GetTipColumnKey(int offset) => $"T{offset:X2}";

        private void SubscribeToTarget()
        {
            if (this.target != null)
            {
                // Unsubscribing first keeps this idempotent, so a reload never leaves two handlers behind.
                this.target.Changed -= this.OnTargetChanged;
                this.target.Changed += this.OnTargetChanged;
            }
        }

        private void UnsubscribeFromTarget()
        {
            if (this.target != null)
            {
                this.target.Changed -= this.OnTargetChanged;
            }
        }

        private void OnTargetChanged(object? sender, ByteEditTargetChangedEventArgs e)
        {
            // Edits made here refresh the grid themselves, once the cell edit that caused them has finished.
            if (e.IsLocal)
            {
                return;
            }

            if (this.isEditingCell)
            {
                // Never change the grid under a cell the user is typing in; catch up when the edit ends.
                this.hasPendingSync = true;
                return;
            }

            this.SyncFromTarget();
        }

        private void BuildGridColumns()
        {
            this.bytesGrid.Columns.Clear();

            this.bytesGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Address",
                Binding = new Binding("Address"),
                Width = 190,
                MinWidth = 190,
                IsReadOnly = true,
            });

            for (var i = 0; i < BytesPerRow; i++)
            {
                this.bytesGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = i.ToString("X2"),
                    Binding = new Binding(GetByteColumnKey(i)),
                    Width = 44,
                    CellStyle = this.CreateByteCellStyle(i),
                });
            }

            this.bytesGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "ASCII",
                Binding = new Binding("ASCII"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),

                // 16 characters of the grid's font, plus padding; the grid scrolls sideways when the page is narrower.
                MinWidth = 210,
            });
        }

        private Style CreateByteCellStyle(int column)
        {
            var baseStyle = this.TryFindResource(typeof(DataGridCell)) as Style;
            var style = new Style(typeof(DataGridCell), baseStyle);

            style.Setters.Add(new Setter(
                Control.BackgroundProperty,
                new Binding(GetRegionColumnKey(column)) { Converter = HexPalette.KindToBrush }));
            style.Setters.Add(new Setter(
                FrameworkElement.ToolTipProperty,
                new Binding(GetTipColumnKey(column)) { Converter = HexPalette.TextOrNull }));

            var changed = new DataTrigger { Binding = new Binding(GetChangedColumnKey(column)), Value = "1" };
            changed.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
            changed.Setters.Add(new Setter(Control.ForegroundProperty, HexPalette.ChangedForeground));
            changed.Setters.Add(new Setter(Control.BorderBrushProperty, HexPalette.ChangedLine));
            changed.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0, 0, 0, 3)));
            style.Triggers.Add(changed);

            var selected = new Trigger { Property = DataGridCell.IsSelectedProperty, Value = true };
            selected.Setters.Add(new Setter(Control.BackgroundProperty, HexPalette.SelectedBackground));
            selected.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.Black));
            style.Triggers.Add(selected);

            return style;
        }

        /// <summary>
        /// Reads everything from the target again and builds the grid from scratch.
        /// </summary>
        private void RebuildGrid()
        {
            if (this.target == null)
            {
                return;
            }

            this.bytes = this.target.GetBytes();
            this.original = this.target.GetOriginalBytes();
            this.map = new ByteRegionMap(this.target.Regions, this.bytes.Length);
            this.titleText.Text = this.target.Title;
            this.BuildRegionList();

            this.isRebuilding = true;
            try
            {
                var newTable = new DataTable();
                newTable.Columns.Add("Address", typeof(string));
                for (var i = 0; i < BytesPerRow; i++)
                {
                    newTable.Columns.Add(GetByteColumnKey(i), typeof(string));
                }

                newTable.Columns.Add("ASCII", typeof(string));
                for (var i = 0; i < BytesPerRow; i++)
                {
                    newTable.Columns.Add(GetRegionColumnKey(i), typeof(string));
                    newTable.Columns.Add(GetChangedColumnKey(i), typeof(string));
                    newTable.Columns.Add(GetTipColumnKey(i), typeof(string));
                }

                for (var rowStart = 0; rowStart < this.bytes.Length; rowStart += BytesPerRow)
                {
                    var row = newTable.NewRow();
                    this.FillRow(row, rowStart);
                    newTable.Rows.Add(row);
                }

                this.table = newTable;
                this.bytesGrid.ItemsSource = this.table.DefaultView;

                if (this.bytesGrid.Items.Count > 0 && this.bytesGrid.Columns.Count > FirstByteColumnIndex)
                {
                    this.bytesGrid.SelectedCells.Clear();
                    this.bytesGrid.CurrentCell = new DataGridCellInfo(this.bytesGrid.Items[0], this.bytesGrid.Columns[FirstByteColumnIndex]);
                    this.bytesGrid.SelectedCells.Add(this.bytesGrid.CurrentCell);
                }
            }
            finally
            {
                this.isRebuilding = false;
            }

            this.UpdateChangedSummary();
            this.UpdateStatusBar();
        }

        /// <summary>
        /// Brings the grid in line with the target's current bytes by updating only the rows that differ.
        /// The selection and scroll position are untouched.
        /// </summary>
        private void SyncFromTarget()
        {
            if (this.target == null)
            {
                return;
            }

            var latest = this.target.GetBytes();
            if (this.table == null || latest.Length != this.bytes.Length)
            {
                this.RebuildGrid();
                return;
            }

            // Saving moves what "changed" is measured against, which changes how every row looks.
            var latestOriginal = this.target.GetOriginalBytes();
            var originalMoved = !latestOriginal.AsSpan().SequenceEqual(this.original);
            this.original = latestOriginal;

            var differences = ByteEditTargetExtensions.FindDifferences(this.bytes, latest);
            this.bytes = latest;
            this.titleText.Text = this.target.Title;

            var rows = originalMoved
                ? Enumerable.Range(0, this.table.Rows.Count)
                : differences.Select(i => i / BytesPerRow).Distinct();

            foreach (var rowIndex in rows)
            {
                this.RefreshRow(rowIndex);
            }

            this.UpdateChangedSummary();
            this.UpdateStatusBar();
        }

        private void RefreshRow(int rowIndex)
        {
            if (this.table == null || rowIndex < 0 || rowIndex >= this.table.Rows.Count)
            {
                return;
            }

            this.FillRow(this.table.Rows[rowIndex], rowIndex * BytesPerRow);
        }

        private void FillRow(DataRow row, int rowStart)
        {
            row["Address"] = $"0x{rowStart:X8} ({rowStart})";

            var chars = new StringBuilder(BytesPerRow);
            for (var i = 0; i < BytesPerRow; i++)
            {
                var index = rowStart + i;
                if (index < this.bytes.Length)
                {
                    var value = this.bytes[index];
                    var isChanged = index < this.original.Length && this.original[index] != value;
                    var leaf = this.map.FindLeaf(index);

                    row[GetByteColumnKey(i)] = value.ToString("X2");
                    row[GetRegionColumnKey(i)] = leaf == null ? string.Empty : $"{leaf.Kind}{this.map.GetShade(index)}";
                    row[GetChangedColumnKey(i)] = isChanged ? "1" : string.Empty;
                    row[GetTipColumnKey(i)] = this.DescribeByte(index);
                    chars.Append(HexFormat.ToAsciiPreview(value));
                }
                else
                {
                    row[GetByteColumnKey(i)] = string.Empty;
                    row[GetRegionColumnKey(i)] = string.Empty;
                    row[GetChangedColumnKey(i)] = string.Empty;
                    row[GetTipColumnKey(i)] = string.Empty;
                    chars.Append(' ');
                }
            }

            row["ASCII"] = chars.ToString();
        }

        private string DescribeByte(int index)
        {
            var lines = new List<string>();
            foreach (var region in this.map.FindAll(index))
            {
                lines.Add($"{region.FullName} ({RegionNode.DescribeRange(region)})");
                if (!string.IsNullOrEmpty(region.Description))
                {
                    lines.Add(region.Description);
                }
            }

            if (index < this.original.Length && this.original[index] != this.bytes[index])
            {
                lines.Add($"Changed: was {this.original[index]:X2}, now {this.bytes[index]:X2}");
            }

            return string.Join(Environment.NewLine, lines);
        }

        private void UpdateChangedSummary()
        {
            var count = ByteEditTargetExtensions.FindDifferences(this.original, this.bytes).Count;
            this.changedText.Text = count switch
            {
                0 => "No bytes changed",
                1 => "1 byte changed",
                _ => $"{count} bytes changed",
            };
        }

        private void BuildRegionList()
        {
            if (this.target == null)
            {
                return;
            }

            // One list of a single node per field, which the page shows as a tree of its own.
            this.regionList.ItemsSource = this.target.Regions.Select(r => new[] { RegionNode.From(r, 0) }).ToList();
            this.legend.ItemsSource = this.target.Regions
                .Select(r => r.Kind)
                .Distinct()
                .Select(k => new RegionNode(HexPalette.GetLabel(k), string.Empty, HexPalette.GetBrush(k, 0), null, []))
                .ToList();
        }

        private void RegionSelected(object sender, RoutedEventArgs e)
        {
            // Selected bubbles up to the parent items too; only act on the item that was clicked.
            if (!ReferenceEquals(e.OriginalSource, sender) || sender is not TreeViewItem item || item.DataContext is not RegionNode node || node.Region == null)
            {
                return;
            }

            this.SelectRegion(node.Region);

            // Deselect again so that clicking the same field a second time selects its bytes once more.
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => item.IsSelected = false));
        }

        private void SelectRegion(ByteRegion region)
        {
            if (this.bytesGrid.Items.Count == 0 || region.Start >= this.bytes.Length)
            {
                return;
            }

            this.isRebuilding = true;
            this.selectionAnchor = region.Start;
            try
            {
                this.bytesGrid.SelectedCells.Clear();
                for (var index = region.Start; index < region.End && index < this.bytes.Length; index++)
                {
                    var cell = this.ToCellInfo(index);
                    this.bytesGrid.SelectedCells.Add(cell);
                    if (index == region.Start)
                    {
                        this.bytesGrid.CurrentCell = cell;
                    }
                }

                this.bytesGrid.ScrollIntoView(this.bytesGrid.Items[region.Start / BytesPerRow], this.bytesGrid.Columns[FirstByteColumnIndex]);
            }
            finally
            {
                this.isRebuilding = false;
            }

            this.bytesGrid.Focus();
            this.UpdateStatusBar();
        }

        private DataGridCellInfo ToCellInfo(int index)
        {
            return new DataGridCellInfo(
                this.bytesGrid.Items[index / BytesPerRow],
                this.bytesGrid.Columns[FirstByteColumnIndex + (index % BytesPerRow)]);
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            if (this.target == null || this.isEditingCell)
            {
                return;
            }

            this.RebuildGrid();
        }

        private async void ExportClick(object sender, RoutedEventArgs e)
        {
            if (this.target == null)
            {
                return;
            }

            await this.ExportBytesAsync(Window.GetWindow(this));
        }

        private async System.Threading.Tasks.Task ExportBytesAsync(Window owner)
        {
            if (this.target == null)
            {
                return;
            }

            var invalid = System.IO.Path.GetInvalidFileNameChars();
            var safeName = string.Concat(this.target.Title.Select(c => invalid.Contains(c) ? '_' : c));
            var saveFile = GetSourceFile(this);
            var saveName = string.IsNullOrWhiteSpace(saveFile)
                ? string.Empty
                : string.Concat(System.IO.Path.GetFileNameWithoutExtension(saveFile).Select(c => invalid.Contains(c) ? '_' : c)) + "_";
            var saveDlg = new SaveFileDialog
            {
                Filter = "Binary dump|*.bin|All files|*.*",
                DefaultExt = ".bin",
                AddExtension = true,
                FileName = $"{saveName}{safeName}.bin",
            };

            if (saveDlg.ShowDialog(owner) != true)
            {
                return;
            }

            try
            {
                await System.IO.File.WriteAllBytesAsync(saveDlg.FileName, this.target.GetBytes());
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, $"Failed to export the bytes: {ex.Message}", "Export bytes", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BytesGridSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (this.isRebuilding || this.isNormalizingSelection)
            {
                return;
            }

            this.NormalizeSelection();
            this.UpdateStatusBar();
        }

        /// <summary>
        /// The grid selects a rectangle when the user drags or shift-clicks across rows. Bytes read left to right, row after
        /// row, so this turns the rectangle into the run of bytes between the cell the selection started at and the one it ends at.
        /// </summary>
        private void NormalizeSelection()
        {
            // Ctrl-click builds a selection by hand, and a cell being edited is not a selection.
            if (this.isEditingCell || Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || this.bytes.Length == 0)
            {
                return;
            }

            var cells = new List<(int Row, int Column)>();
            foreach (var cell in this.bytesGrid.SelectedCells)
            {
                if (!this.TryGetByteCell(cell, out var row, out var column))
                {
                    // Address and ASCII cells are selected as they are.
                    return;
                }

                cells.Add((row, column));
            }

            if (cells.Count == 0)
            {
                return;
            }

            if (cells.Count == 1)
            {
                // A click, or an arrow key, starts a new selection here.
                this.selectionAnchor = (cells[0].Row * BytesPerRow) + cells[0].Column;
                return;
            }

            var minRow = cells.Min(c => c.Row);
            var maxRow = cells.Max(c => c.Row);
            var minColumn = cells.Min(c => c.Column);
            var maxColumn = cells.Max(c => c.Column);

            if (this.selectionAnchor < 0 || this.selectionAnchor >= this.bytes.Length)
            {
                this.selectionAnchor = (minRow * BytesPerRow) + minColumn;
            }

            // The rectangle has the anchor at one corner; the opposite corner is where the selection ends.
            var anchorRow = this.selectionAnchor / BytesPerRow;
            var anchorColumn = this.selectionAnchor % BytesPerRow;
            var endRow = anchorRow == minRow ? maxRow : minRow;
            var endColumn = anchorColumn == minColumn ? maxColumn : minColumn;
            var end = (endRow * BytesPerRow) + endColumn;

            var first = Math.Min(this.selectionAnchor, end);
            var last = Math.Min(Math.Max(this.selectionAnchor, end), this.bytes.Length - 1);

            if (cells.Count == last - first + 1 && cells.All(c => (c.Row * BytesPerRow) + c.Column >= first && (c.Row * BytesPerRow) + c.Column <= last))
            {
                return;
            }

            this.isNormalizingSelection = true;
            try
            {
                this.bytesGrid.SelectedCells.Clear();
                for (var index = first; index <= last; index++)
                {
                    this.bytesGrid.SelectedCells.Add(this.ToCellInfo(index));
                }
            }
            finally
            {
                this.isNormalizingSelection = false;
            }
        }

        private bool TryGetByteCell(DataGridCellInfo cell, out int row, out int column)
        {
            row = -1;
            column = -1;

            if (cell.Item is not DataRowView rowView || cell.Column == null)
            {
                return false;
            }

            var columnIndex = cell.Column.DisplayIndex;
            row = this.bytesGrid.Items.IndexOf(rowView);
            column = columnIndex - FirstByteColumnIndex;
            return row >= 0 && columnIndex >= FirstByteColumnIndex && columnIndex <= LastByteColumnIndex;
        }

        /// <summary>
        /// Gets how many bytes are selected, counting each byte once.
        /// </summary>
        private int CountSelectedBytes()
        {
            return this.bytesGrid.SelectedCells
                .Where(cell => this.TryGetByteCell(cell, out _, out _))
                .Select(this.ToByteIndex)
                .Where(index => index.HasValue && index.Value < this.bytes.Length)
                .Distinct()
                .Count();
        }

        private void BytesGridCurrentCellChanged(object sender, EventArgs e)
        {
            if (!this.isRebuilding)
            {
                this.UpdateStatusBar();
            }
        }

        private void BytesGridBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            this.isEditingCell = true;
        }

        private void BytesGridCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                this.HandleCellEdit(e);
            }
            finally
            {
                this.isEditingCell = false;
                if (this.hasPendingSync)
                {
                    // Something changed elsewhere while the cell was being edited; show it once the edit is committed.
                    this.hasPendingSync = false;
                    this.ScheduleRefresh(-1);
                }
            }
        }

        private void HandleCellEdit(DataGridCellEditEndingEventArgs e)
        {
            if (this.isRebuilding || this.target == null || e.EditAction != DataGridEditAction.Commit)
            {
                return;
            }

            if (e.Row.Item is not DataRowView rowView || e.EditingElement is not TextBox textBox)
            {
                return;
            }

            var rowIndex = this.bytesGrid.Items.IndexOf(rowView);
            if (rowIndex < 0)
            {
                return;
            }

            var columnIndex = e.Column.DisplayIndex;
            if (columnIndex >= FirstByteColumnIndex && columnIndex <= LastByteColumnIndex)
            {
                this.CommitHexEdit(rowIndex, columnIndex, textBox.Text);
            }
            else if (columnIndex == AsciiColumnIndex)
            {
                this.CommitAsciiEdit(rowIndex, textBox.Text ?? string.Empty);
            }
        }

        private void CommitHexEdit(int rowIndex, int columnIndex, string input)
        {
            if (this.target == null)
            {
                return;
            }

            var absoluteIndex = (rowIndex * BytesPerRow) + (columnIndex - FirstByteColumnIndex);
            if (absoluteIndex < 0 || absoluteIndex >= this.bytes.Length)
            {
                return;
            }

            var text = (input ?? string.Empty).Trim();
            if (text.Length == 0 || text.Length > 2 || !byte.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
            {
                MessageBox.Show(Window.GetWindow(this), "Use 1 or 2 hex digits (00-FF).", "Invalid byte", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                this.target.WriteBytes(absoluteIndex, [value]);
            }

            // The grid writes the typed text into the cell after this handler returns, so tidy it up afterwards.
            this.ScheduleRefresh(rowIndex);
        }

        private void CommitAsciiEdit(int rowIndex, string input)
        {
            if (this.target == null)
            {
                return;
            }

            var rowStart = rowIndex * BytesPerRow;
            if (rowStart < 0 || rowStart >= this.bytes.Length)
            {
                return;
            }

            // Only the characters that were actually changed are written, so the dots that stand for
            // unprintable bytes in the preview do not overwrite them. The whole edit is one undo step.
            var values = new List<(int Index, byte Value)>();
            for (var i = 0; i < input.Length && i < BytesPerRow && rowStart + i < this.bytes.Length; i++)
            {
                if (input[i] != HexFormat.ToAsciiPreview(this.bytes[rowStart + i]))
                {
                    values.Add((rowStart + i, input[i] <= 127 ? (byte)input[i] : (byte)'?'));
                }
            }

            if (values.Count > 0)
            {
                this.target.Update(current =>
                {
                    foreach (var (index, value) in values)
                    {
                        current[index] = value;
                    }

                    return current;
                });
            }

            this.ScheduleRefresh(rowIndex);
        }

        /// <summary>
        /// Brings the grid up to date once the current event has finished, and rewrites the given row (-1 for none).
        /// </summary>
        private void ScheduleRefresh(int rowIndex)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (this.target == null)
                {
                    return;
                }

                if (this.isEditingCell)
                {
                    this.hasPendingSync = true;
                    return;
                }

                this.SyncFromTarget();
                this.RefreshRow(rowIndex);
            }));
        }

        private void UpdateStatusBar()
        {
            this.statusSelected.Text = $"Selected: {(this.bytes.Length == 0 ? 0 : this.CountSelectedBytes())}";

            var anchorIndex = this.bytes.Length == 0 ? null : this.GetAnchorByteIndex();
            if (!anchorIndex.HasValue)
            {
                this.statusAddress.Text = "Addr: -";
                this.statusChar.Text = "Char: -";
                this.statusStringChar.Text = "String[0]: -";
                this.statusU8.Text = "u8: -";
                this.statusU16.Text = "u16: -";
                this.statusU32.Text = "u32: -";
                this.statusRegion.Text = "Field: -";
                return;
            }

            var index = anchorIndex.Value;
            var value = this.bytes[index];
            this.statusAddress.Text = $"Addr: 0x{index:X8} ({index})";
            this.statusChar.Text = $"Char: {HexFormat.DescribeChar(value)}";
            this.statusStringChar.Text = $"String[0]: {this.DescribeFirstCharOfAsciiString(index)}";
            this.statusU8.Text = $"u8: {value}";
            this.statusU16.Text = $"u16: {this.ReadUIntLittleEndian(index, 2)}";
            this.statusU32.Text = $"u32: {this.ReadUIntLittleEndian(index, 4)}";

            var regions = this.map.FindAll(index);
            this.statusRegion.Text = regions.Count == 0
                ? "Field: unknown"
                : $"Field: {string.Join(" / ", regions.Select(r => $"{r.FullName} ({RegionNode.DescribeRange(r)})"))}";
        }

        private int? GetAnchorByteIndex()
        {
            var selectedByteIndexes = this.bytesGrid.SelectedCells
                .Select(this.ToByteIndex)
                .OfType<int>()
                .Where(index => index >= 0 && index < this.bytes.Length)
                .ToList();

            if (selectedByteIndexes.Count > 0)
            {
                return selectedByteIndexes.Min();
            }

            var current = this.ToByteIndex(this.bytesGrid.CurrentCell);
            if (current.HasValue && current.Value >= 0 && current.Value < this.bytes.Length)
            {
                return current.Value;
            }

            return null;
        }

        private int? ToByteIndex(DataGridCellInfo cell)
        {
            if (cell.Item is not DataRowView rowView || cell.Column == null)
            {
                return null;
            }

            var rowIndex = this.bytesGrid.Items.IndexOf(rowView);
            if (rowIndex < 0)
            {
                return null;
            }

            var columnIndex = cell.Column.DisplayIndex;
            if (columnIndex >= FirstByteColumnIndex && columnIndex <= LastByteColumnIndex)
            {
                return (rowIndex * BytesPerRow) + (columnIndex - FirstByteColumnIndex);
            }

            if (columnIndex == AsciiColumnIndex || columnIndex == AddressColumnIndex)
            {
                return rowIndex * BytesPerRow;
            }

            return null;
        }

        private string DescribeFirstCharOfAsciiString(int index)
        {
            if (index < 0 || index >= this.bytes.Length || this.bytes[index] == 0)
            {
                return index < 0 || index >= this.bytes.Length ? "-" : "N/A";
            }

            return HexFormat.DescribeChar(this.bytes[index]);
        }

        private string ReadUIntLittleEndian(int index, int targetBytes)
        {
            if (index < 0 || index >= this.bytes.Length)
            {
                return "-";
            }

            var available = Math.Min(targetBytes, this.bytes.Length - index);
            ulong value = 0;
            for (var i = 0; i < available; i++)
            {
                value |= (ulong)this.bytes[index + i] << (i * 8);
            }

            if (available == targetBytes)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }

            return $"{value} (partial {available}/{targetBytes})";
        }
    }
}
