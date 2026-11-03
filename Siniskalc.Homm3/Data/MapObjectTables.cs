namespace Siniskalc.Homm3.Data
{
    /// <summary>The independently validated starts and counts of the owner/guard and creature-generator tables.</summary>
    public sealed record MapObjectTables(int OwnerGuardStart, int OwnerGuardCount, int GeneratorStart, int GeneratorCount)
    {
        /// <summary>Gets the exclusive end of the last generator's known payload, without the following block's byte.</summary>
        public int GeneratorEnd => this.GeneratorStart + (this.GeneratorCount * DwellingLayout.RecordSize)
            - (this.GeneratorCount > 0 ? DwellingLayout.RecordSize - DwellingLayout.EditableSize : 0);
    }
}
