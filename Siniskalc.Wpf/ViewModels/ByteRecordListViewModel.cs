namespace Siniskalc.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;

    /// <summary>
    /// The records a Hex tab can show and the one that is picked.
    /// </summary>
    public sealed class ByteRecordListViewModel : AbstractViewModelBase
    {
        private ByteRecordViewModel? selectedRecord;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteRecordListViewModel"/> class; the first record is picked.
        /// </summary>
        /// <param name="records">The records the tab lists.</param>
        public ByteRecordListViewModel(IReadOnlyList<ByteRecordViewModel> records)
        {
            this.Records = records;
            this.selectedRecord = records.FirstOrDefault();
        }

        /// <summary>
        /// Gets the records whose raw bytes the tab can show.
        /// </summary>
        public IReadOnlyList<ByteRecordViewModel> Records { get; }

        /// <summary>
        /// Gets or sets the record the tab shows.
        /// </summary>
        public ByteRecordViewModel? SelectedRecord
        {
            get => this.selectedRecord;
            set
            {
                if (value != this.selectedRecord)
                {
                    this.selectedRecord = value;
                    this.RaisePropertyChanged(nameof(this.SelectedRecord));
                }
            }
        }

        /// <summary>
        /// Creates the raw bytes of the record that is picked, or <c>null</c> when none is. The caller owns the target and disposes it.
        /// </summary>
        /// <returns>The target, or <see langword="null"/> when no record is picked.</returns>
        public IByteEditTarget? CreateTarget() => this.SelectedRecord?.Create();
    }
}
