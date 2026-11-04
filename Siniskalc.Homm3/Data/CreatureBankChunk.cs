namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Models;

    /// <summary>A bank record and its entrance state; an occupied entrance belongs to the existing hero chunk instead.</summary>
    public sealed class CreatureBankChunk(string name, byte[] bytes, int start, int index, int bankIndex, MapPosition position, CreatureBankKind kind, RawBytesChunk entrance, int? occupyingHero)
        : RecordChunk(name, bytes, start, index)
    {
        public int BankIndex { get; } = bankIndex;

        public MapPosition Position { get; } = position;

        public CreatureBankKind Kind { get; } = kind;

        public RawBytesChunk Entrance { get; } = entrance;

        public int? OccupyingHero { get; } = occupyingHero;

        public override IEnumerable<BytesChunk> PatchParts
        {
            get
            {
                yield return this;
                if (this.OccupyingHero == null)
                {
                    yield return this.Entrance;
                }
            }
        }

        public override CreatureBankChunk WithBytes(byte[] bytes) =>
            new(this.Name, this.CheckReplacement(bytes), this.StartIndex, this.Index, this.BankIndex, this.Position, this.Kind, this.Entrance, this.OccupyingHero);

        public CreatureBankChunk WithEntrance(byte[] bytes) =>
            new(this.Name, this.ToArray(), this.StartIndex, this.Index, this.BankIndex, this.Position, this.Kind, this.Entrance.WithBytes(bytes), this.OccupyingHero);

        public override bool HasSameBytes(BytesChunk other) => base.HasSameBytes(other)
            && other is CreatureBankChunk bank && this.Entrance.HasSameBytes(bank.Entrance);
    }
}
