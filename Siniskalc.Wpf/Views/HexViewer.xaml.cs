namespace Siniskalc.Wpf.Views
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Siniskalc.Core.Data;

    /// <summary>
    /// A read-only hex and ASCII view of a <see cref="SaveHexDump"/>: it colours the bytes of known regions, marks bytes that
    /// differ from the savefile as loaded, and lists the regions so they can be selected. It is made for a whole savefile,
    /// so the lines of the grid are only made when they are on screen (see <see cref="HexRowList"/>). Use the
    /// <see cref="HexEditor"/> to change bytes.
    /// </summary>
    public partial class HexViewer : UserControl
    {
        private const int BytesPerRow = HexRowList.BytesPerRow;
        private const int FirstByteColumnIndex = 1;
        private const int LastByteColumnIndex = 16;
        private const int AsciiColumnIndex = 17;

        // Selecting a field selects its bytes cell by cell, which is slow for thousands of them.
        private const int MaxSelectedBytes = 4096;

        // Counting the selection cell by cell is just as slow after a select all.
        private const int MaxCountedCells = 100000;

        private Func<SaveHexDump>? source;
        private byte[] bytes = [];
        private ByteRegionMap map = new([], 0);
        private bool isSelecting;

        public HexViewer()
        {
            this.InitializeComponent();
            this.BuildGridColumns();
            this.bytesGrid.ClipboardCopyMode = DataGridClipboardCopyMode.ExcludeHeader;
        }

        /// <summary>
        /// Shows the savefile that <paramref name="dumpSource"/> gives, and again whenever the view is refreshed.
        /// </summary>
        public void Bind(Func<SaveHexDump> dumpSource)
        {
            this.source = dumpSource;
            this.Reload();
        }

        /// <summary>
        /// Reads the savefile again, so that it shows the edits made since.
        /// </summary>
        public void Reload()
        {
            if (this.source == null)
            {
                return;
            }

            var dump = this.source();
            this.bytes = dump.Bytes;

            this.map = new ByteRegionMap(dump.Regions, dump.Bytes.Length);
            this.titleText.Text = dump.Title;
            this.regionTree.ItemsSource = dump.Regions.Select(r => RegionNode.From(r, 0)).ToList();
            this.legend.ItemsSource = dump.Regions
                .SelectMany(r => r.Leaves())
                .Select(l => l.Kind)
                .Distinct()
                .OrderBy(k => k)
                .Select(k => new RegionNode(HexPalette.GetLabel(k), string.Empty, HexPalette.GetBrush(k, 0), null, []))
                .ToList();

            var changed = ByteEditTargetExtensions.FindDifferences(dump.OriginalBytes, dump.Bytes).Count;
            this.changedText.Text = changed switch
            {
                0 => "No bytes changed",
                1 => "1 byte changed",
                _ => $"{changed} bytes changed",
            };

            this.isSelecting = true;
            try
            {
                this.bytesGrid.ItemsSource = new HexRowList(dump.Bytes, dump.OriginalBytes, this.map);
            }
            finally
            {
                this.isSelecting = false;
            }

            this.UpdateStatusBar();
        }

        private void BuildGridColumns()
        {
            this.bytesGrid.Columns.Clear();

            this.bytesGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Address",
                Binding = new Binding(nameof(HexRow.Address)),
                Width = 190,
                MinWidth = 190,
            });

            for (var i = 0; i < BytesPerRow; i++)
            {
                this.bytesGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = i.ToString("X2"),
                    Binding = new Binding($"Cells[{i}].Text"),
                    Width = 44,
                    CellStyle = this.CreateByteCellStyle(i),
                });
            }

            this.bytesGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "ASCII",
                Binding = new Binding(nameof(HexRow.Ascii)),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),

                // 16 characters of the grid's font, plus padding; the grid scrolls sideways when the window is narrower.
                MinWidth = 210,
            });
        }

        private Style CreateByteCellStyle(int column)
        {
            var baseStyle = this.TryFindResource(typeof(DataGridCell)) as Style;
            var style = new Style(typeof(DataGridCell), baseStyle);

            style.Setters.Add(new Setter(Control.BackgroundProperty, new Binding($"Cells[{column}].Background")));
            style.Setters.Add(new Setter(FrameworkElement.ToolTipProperty, new Binding($"Cells[{column}].ToolTip")));

            var changed = new DataTrigger { Binding = new Binding($"Cells[{column}].IsChanged"), Value = true };
            changed.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
            changed.Setters.Add(new Setter(Control.ForegroundProperty, HexPalette.ChangedForeground));
            changed.Setters.Add(new Setter(Control.BorderBrushProperty, HexPalette.ChangedLine));
            changed.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0, 0, 0, 3)));
            style.Triggers.Add(changed);

            var selected = new Trigger { Property = DataGridCell.IsSelectedProperty, Value = true };
            selected.Setters.Add(new Setter(Control.BackgroundProperty, HexPalette.SelectedBackground));
            selected.Setters.Add(new Setter(Control.ForegroundProperty, System.Windows.Media.Brushes.Black));
            style.Triggers.Add(selected);

            return style;
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            this.Reload();
        }

        private void GotoBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            var text = this.gotoBox.Text.Trim();
            var isHex = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
            var parsed = isHex
                ? int.TryParse(text.AsSpan(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var offset)
                : int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out offset);

            if (parsed && offset < this.bytes.Length)
            {
                this.SelectBytes(offset, 1);
            }
            else
            {
                this.statusRegion.Text = $"Field: there is no byte at \"{text}\" (the savefile has {this.bytes.Length} bytes)";
            }

            e.Handled = true;
        }

        private void RegionSelected(object sender, RoutedEventArgs e)
        {
            // Selected bubbles up to the parent items too; only act on the item that was clicked.
            if (!ReferenceEquals(e.OriginalSource, sender) || sender is not TreeViewItem item || item.DataContext is not RegionNode node || node.Region == null)
            {
                return;
            }

            this.SelectBytes(node.Region.Start, node.Region.Length);

            // Deselect again so that clicking the same field a second time selects its bytes once more.
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => item.IsSelected = false));
        }

        /// <summary>
        /// Selects the bytes from <paramref name="start"/> on, as many as can be selected, and scrolls to them.
        /// </summary>
        private void SelectBytes(int start, int length)
        {
            if (this.bytesGrid.Items.Count == 0 || start < 0 || start >= this.bytes.Length)
            {
                return;
            }

            var end = Math.Min(start + Math.Min(length, MaxSelectedBytes), this.bytes.Length);

            this.isSelecting = true;
            try
            {
                this.bytesGrid.SelectedCells.Clear();
                for (var index = start; index < end; index++)
                {
                    var cell = this.ToCellInfo(index);
                    this.bytesGrid.SelectedCells.Add(cell);
                    if (index == start)
                    {
                        this.bytesGrid.CurrentCell = cell;
                    }
                }

                this.bytesGrid.ScrollIntoView(this.bytesGrid.Items[start / BytesPerRow], this.bytesGrid.Columns[FirstByteColumnIndex]);
            }
            finally
            {
                this.isSelecting = false;
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

        private void BytesGridSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (!this.isSelecting)
            {
                this.UpdateStatusBar();
            }
        }

        private void BytesGridCurrentCellChanged(object sender, EventArgs e)
        {
            if (!this.isSelecting)
            {
                this.UpdateStatusBar();
            }
        }

        private void UpdateStatusBar()
        {
            this.statusSelected.Text = $"Selected: {this.DescribeSelectedCount()}";

            var anchor = this.bytes.Length == 0 ? null : this.GetCurrentByteIndex();
            if (!anchor.HasValue)
            {
                this.statusAddress.Text = "Addr: -";
                this.statusChar.Text = "Char: -";
                this.statusU8.Text = "u8: -";
                this.statusU16.Text = "u16: -";
                this.statusU32.Text = "u32: -";
                this.statusRegion.Text = "Field: -";
                return;
            }

            var index = anchor.Value;
            var value = this.bytes[index];
            this.statusAddress.Text = $"Addr: 0x{index:X8} ({index})";
            this.statusChar.Text = $"Char: {HexFormat.DescribeChar(value)}";
            this.statusU8.Text = $"u8: {value}";
            this.statusU16.Text = $"u16: {this.ReadUIntLittleEndian(index, 2)}";
            this.statusU32.Text = $"u32: {this.ReadUIntLittleEndian(index, 4)}";

            var regions = this.map.FindAll(index);
            this.statusRegion.Text = regions.Count == 0
                ? "Field: unknown"
                : $"Field: {string.Join(" / ", regions.Select(r => $"{r.FullName} ({RegionNode.DescribeRange(r)})"))}";
        }

        private string DescribeSelectedCount()
        {
            var cells = this.bytesGrid.SelectedCells;
            if (this.bytes.Length == 0 || cells.Count == 0)
            {
                return "0";
            }

            if (cells.Count > MaxCountedCells)
            {
                return $"{cells.Count} cells";
            }

            return cells.Count(cell => this.ToByteIndex(cell, onlyBytes: true).HasValue).ToString(CultureInfo.InvariantCulture);
        }

        private int? GetCurrentByteIndex()
        {
            var index = this.ToByteIndex(this.bytesGrid.CurrentCell, onlyBytes: false);
            return index.HasValue && index.Value < this.bytes.Length ? index : null;
        }

        /// <summary>
        /// Returns the offset of the byte in <paramref name="cell"/>. A cell of the address or the text stands for the first
        /// byte of its line, unless <paramref name="onlyBytes"/> is set, which gives nothing for them.
        /// </summary>
        private int? ToByteIndex(DataGridCellInfo cell, bool onlyBytes)
        {
            if (cell.Item is not HexRow row || cell.Column == null)
            {
                return null;
            }

            var columnIndex = cell.Column.DisplayIndex;
            if (columnIndex >= FirstByteColumnIndex && columnIndex <= LastByteColumnIndex)
            {
                return (row.Index * BytesPerRow) + (columnIndex - FirstByteColumnIndex);
            }

            return !onlyBytes && columnIndex is 0 or AsciiColumnIndex ? row.Index * BytesPerRow : null;
        }

        private string ReadUIntLittleEndian(int index, int targetBytes)
        {
            var available = Math.Min(targetBytes, this.bytes.Length - index);
            ulong value = 0;
            for (var i = 0; i < available; i++)
            {
                value |= (ulong)this.bytes[index + i] << (i * 8);
            }

            return available == targetBytes
                ? value.ToString(CultureInfo.InvariantCulture)
                : $"{value} (partial {available}/{targetBytes})";
        }
    }
}
