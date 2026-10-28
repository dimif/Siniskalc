using Moq;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Models
{
    /// <summary>Heroes built in memory, so a view model can be tested without a savefile.</summary>
    internal static class HeroModels
    {
        /// <summary>A Blue Knight of level 5 standing on the surface with ten pikemen, no artifacts, no spells and nothing visited.</summary>
        public static HeroModel Christian()
        {
            var resolver = new Mock<IGrantedSpellsResolver>();
            resolver.Setup(r => r.ResolveGrantedSpells(It.IsAny<EquippedArtifacts>())).Returns(new Dictionary<Spell, string>());
            var equipped = new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>());
            return new HeroModel(
                21,
                "Christian",
                PlayerColor.Blue,
                new HeroPosition(12, 7, MapPosition.Above, FacingDirection.South),
                MapPosition.None,
                new MovementPoints(1000, 1500),
                HeroLevel.AtLevel(LevelCalculator.Table, 5),
                20,
                new PrimarySkills(3, 4, 1, 2),
                new SecondarySkills(SecondarySkill.All.ToDictionary(s => s, _ => SkillLevel.None), []),
                new Army<Creature>(Constants.CreatureSlotCount).WithSlot(0, Creature.Pikeman, 10),
                resolver.Object,
                HeroClass.Knight,
                new Backpack(new BackpackSlot?[Constants.BackpackSlotCount]),
                equipped,
                [],
                new SpellBook(new HashSet<Spell>(), new HashSet<Spell>(), hasSpellBook: true),
                null,
                null,
                false,
                ActiveAdventureSpells.None,
                false,
                0,
                0,
                VisitedBuildingEffects.None,
                PlaceVisits.None);
        }
    }
}
