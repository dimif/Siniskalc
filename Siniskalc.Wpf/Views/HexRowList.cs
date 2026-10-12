namespace Siniskalc.Wpf.Views
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;

    /// <summary>
    /// The lines of a hex grid, made when the grid asks for them.
    /// As an entire Homm 3 savefile is ~ a million bytes, the grid only asks for the lines that are on screen for performance.
    /// </summary>
    internal sealed class HexRowList : IList
    {
        public const int BytesPerRow = 16;

        private const int MaxCachedRows = 1024;

        private readonly byte[] bytes;
        private readonly byte[] original;
        private readonly ByteRegionMap map;
        private readonly Dictionary<int, HexRow> cache = [];

        public HexRowList(byte[] bytes, byte[] original, ByteRegionMap map)
        {
            this.bytes = bytes;
            this.original = original;
            this.map = map;
        }

        public int Count => (this.bytes.Length + BytesPerRow - 1) / BytesPerRow;

        public bool IsReadOnly => true;

        public bool IsFixedSize => true;

        public bool IsSynchronized => false;

        public object SyncRoot => this;

        public object? this[int index]
        {
            get => this.GetRow(index);
            set => throw new NotSupportedException();
        }

        public HexRow GetRow(int index)
        {
            RangeGuard.ThrowIfNotIndex(index, this.Count, nameof(index));

            if (!this.cache.TryGetValue(index, out var row))
            {
                if (this.cache.Count >= MaxCachedRows)
                {
                    this.cache.Clear();
                }

                this.cache[index] = row = this.CreateRow(index);
            }

            return row;
        }

        public int IndexOf(object? value) => value is HexRow row && row.Index < this.Count ? row.Index : -1;

        public bool Contains(object? value) => this.IndexOf(value) >= 0;

        public IEnumerator GetEnumerator()
        {
            for (var i = 0; i < this.Count; i++)
            {
                yield return this.GetRow(i);
            }
        }

        public void CopyTo(Array array, int index)
        {
            foreach (var row in this)
            {
                array.SetValue(row, index++);
            }
        }

        public int Add(object? value) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public void Insert(int index, object? value) => throw new NotSupportedException();

        public void Remove(object? value) => throw new NotSupportedException();

        public void RemoveAt(int index) => throw new NotSupportedException();

        private HexRow CreateRow(int index)
        {
            var rowStart = index * BytesPerRow;
            var cells = new List<HexCell>(BytesPerRow);
            var text = new StringBuilder(BytesPerRow);

            for (var i = 0; i < BytesPerRow; i++)
            {
                var offset = rowStart + i;
                if (offset >= this.bytes.Length)
                {
                    cells.Add(new HexCell(string.Empty, null, null, false));
                    text.Append(' ');
                    continue;
                }

                var value = this.bytes[offset];
                var leaf = this.map.FindLeaf(offset);
                var isChanged = offset < this.original.Length && this.original[offset] != value;
                var brush = leaf == null ? null : HexPalette.GetBrush(leaf.Kind, this.map.GetShade(offset));

                cells.Add(new HexCell(value.ToString("X2"), brush, this.DescribeByte(offset), isChanged));
                text.Append(HexFormat.ToAsciiPreview(value));
            }

            return new HexRow(index, $"0x{rowStart:X8} ({rowStart})", cells, text.ToString());
        }

        private string? DescribeByte(int offset)
        {
            var lines = new List<string>();
            foreach (var region in this.map.FindAll(offset))
            {
                lines.Add($"{region.FullName} ({RegionNode.DescribeRange(region)})");
                if (!string.IsNullOrEmpty(region.Description))
                {
                    lines.Add(region.Description);
                }
            }

            if (offset < this.original.Length && this.original[offset] != this.bytes[offset])
            {
                lines.Add($"Changed: was {this.original[offset]:X2}, now {this.bytes[offset]:X2}");
            }

            return lines.Count == 0 ? null : string.Join(Environment.NewLine, lines);
        }
    }
}
