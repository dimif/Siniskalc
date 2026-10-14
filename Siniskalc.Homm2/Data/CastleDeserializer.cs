namespace Siniskalc.Homm2.Data
{
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a castle. The name is read, never written.
    /// </summary>
    public sealed class CastleDeserializer : ICastleDeserializer
    {
        /// <inheritdoc/>
        public Color? DeserializeOwner(CastleChunk chunk) => Bytes.ReadColor(chunk.GetByteAt(CastleLayout.Owner));

        /// <inheritdoc/>
        public CastleModel Deserialize(CastleChunk chunk)
        {
            var bytes = chunk.ToArray();
            var boatX = chunk.GetByteAt(CastleLayout.Boat);
            var counts = Enumerable.Range(0, Constants.ArmySlotCount).Select(i => (int)chunk.GetUInt16At(CastleLayout.GarrisonCounts + (i * 2))).ToList();

            return new CastleModel(
                chunk.GetByteAt(CastleLayout.Index),
                Bytes.ReadText(bytes, CastleLayout.Name, Constants.Castle.NameLength),
                this.DeserializeOwner(chunk),
                Bytes.ReadFaction(chunk.GetByteAt(CastleLayout.Faction)),
                new Position(chunk.GetByteAt(CastleLayout.X), chunk.GetByteAt(CastleLayout.Y)),
                boatX == Bytes.Empty ? null : new Position(boatX, chunk.GetByteAt(CastleLayout.Boat + 1)),
                Bytes.Army(chunk.GetBytesAt(CastleLayout.GarrisonTypes, Constants.ArmySlotCount), counts),
                Bytes.Id(chunk.GetByteAt(CastleLayout.VisitingHero)),
                (TownBuildings)chunk.GetUInt16At(CastleLayout.TownBuildings),
                (Dwellings)chunk.GetUInt32At(CastleLayout.Dwellings),
                new Numbers(Enumerable.Range(0, Constants.Castle.DwellingCount).Select(i => (int)chunk.GetUInt16At(CastleLayout.Creatures + (i * 2)))),
                new Numbers(Enumerable.Range(0, Constants.Castle.DwellingCount).Select(i => (int)chunk.GetUInt16At(CastleLayout.UpgradedCreatures + (i * 2)))),
                new Numbers(chunk.GetBytesAt(CastleLayout.GuildSpells, Bytes.GuildSpellSlots).Select(Bytes.GuildSpellNumber)),
                new Numbers(chunk.GetBytesAt(CastleLayout.GuildSpellCounts, Constants.Castle.MageGuildLevels).Select(b => (int)b)));
        }
    }
}
