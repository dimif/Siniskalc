namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a hero. A value the editor does not know (a creature or an artifact it has no name for, a colour that is none)
    /// does not stop the load: see <see cref="Bytes"/>. The name is read, never written.
    /// </summary>
    public sealed class HeroDeserializer : IHeroDeserializer
    {
        /// <inheritdoc/>
        public PlayerColor DeserializeOwner(HeroChunk chunk) => Bytes.ReadColor(chunk.GetByteAt(HeroLayout.Owner));

        /// <inheritdoc/>
        public HeroModel Deserialize(HeroChunk chunk)
        {
            var bytes = chunk.ToArray();
            var id = chunk.GetByteAt(HeroLayout.Id);
            var artifacts = new Artifacts(chunk.GetBytesAt(HeroLayout.Artifacts, Constants.Hero.ArtifactSlotCount).Select(Bytes.ReadArtifact));

            return new HeroModel(
                id,
                Bytes.ReadText(bytes, HeroLayout.Name, HeroLayout.NameLength),
                this.DeserializeOwner(chunk),
                new HeroPosition(Bytes.ReadCoordinate(chunk.GetUInt32At(HeroLayout.X)), Bytes.ReadCoordinate(chunk.GetUInt32At(HeroLayout.Y)), Bytes.Facing(chunk.GetByteAt(HeroLayout.Facing))),
                Bytes.Destination(chunk.GetUInt32At(HeroLayout.Destination), chunk.GetUInt32At(HeroLayout.Destination + 4)),
                new MovementPoints(chunk.GetUInt32At(HeroLayout.MovementLeft), chunk.GetUInt32At(HeroLayout.MaxMovement)),
                new HeroLevel(chunk.GetUInt32At(HeroLayout.Experience), chunk.GetByteAt(HeroLayout.Level)),
                chunk.GetUInt16At(HeroLayout.SpellPoints),
                new PrimarySkills(
                    chunk.GetByteAt(HeroLayout.PrimarySkills),
                    chunk.GetByteAt(HeroLayout.PrimarySkills + 1),
                    chunk.GetByteAt(HeroLayout.PrimarySkills + 2),
                    chunk.GetByteAt(HeroLayout.PrimarySkills + 3))
                {
                    // The savefile has the bonus of the artifacts in the skills.
                    Bonus = artifacts.TotalBonus,
                },
                Bytes.SecondarySkills(
                    chunk.GetBytesAt(HeroLayout.SkillLevels, Constants.Hero.SecondarySkillCount),
                    chunk.GetBytesAt(HeroLayout.SkillOrder, Constants.Hero.SecondarySkillCount)),
                ArmyReader.Read(chunk, HeroLayout.Army, CreatureCodes.Instance),
                FactionOf(id),
                artifacts,
                Bytes.SpellBook(chunk.GetBytesAt(HeroLayout.Spells, HeroLayout.SpellsLength)))
            {
                NextBattle = new NextBattleBonus((sbyte)chunk.GetByteAt(HeroLayout.BattleMorale), (sbyte)chunk.GetByteAt(HeroLayout.BattleLuck)),
                Effects = (HeroEffects)chunk.GetUInt32At(HeroLayout.Effects),
                Visited = new VisitedPlaces(
                    chunk.GetUInt32At(HeroLayout.VisitedPlaces),
                    chunk.GetUInt32At(HeroLayout.VisitedPlaces + 4),
                    chunk.GetUInt32At(HeroLayout.VisitedPlaces + 8),
                    chunk.GetUInt32At(HeroLayout.VisitedPlaces + 12),
                    chunk.GetUInt32At(HeroLayout.VisitedPlaces + 16),
                    chunk.GetUInt32At(HeroLayout.VisitedXanadu)),
            };
        }

        // The heroes of each faction are numbered together, nine at a time; a number past the last faction belongs to the last one.
        private static Faction FactionOf(int heroId) => Faction.All[Math.Min(heroId / Constants.Hero.PerFaction, Faction.All.Count - 1)];
    }
}
