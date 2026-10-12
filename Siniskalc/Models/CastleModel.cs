namespace Siniskalc.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Entities;

    /// <summary>
    /// Immutable view of a castle (a town).
    /// </summary>
    /// <param name="Name">The name of the castle.</param>
    /// <param name="Faction">The faction of the castle, or <c>null</c> if the savefile holds one this editor does not know.</param>
    /// <param name="Owner">The player that owns the castle, or <c>null</c> if no player does.</param>
    /// <param name="Position">Where the castle is on the map.</param>
    /// <param name="Garrison">The creatures that guard the castle. Shaped like a hero's army: slots 1 to 7, empty slots absent.</param>
    /// <param name="Buildings">
    /// Every building the castle has built, including the ones an upgrade has replaced (a castle with a capitol also has the
    /// village, town and city hall). Buildings of the savefile that this editor does not know are left out.
    /// </param>
    /// <param name="ShipyardLocation">The tile where the castle's boats appear, or <c>null</c> if the castle is not on the coast. Shown on the same level as the castle.</param>
    /// <param name="GarrisonedHeroId">The id (see <see cref="HeroModel.HeroId"/>) of the hero inside the castle, or <c>null</c>.</param>
    /// <param name="VisitingHeroId">The id of the hero at the gate of the castle, or <c>null</c>.</param>
    /// <param name="CurrentBuildings">The buildings that stand in the castle now: <paramref name="Buildings"/> without the ones an upgrade replaced.</param>
    /// <param name="SpellCounts">How many spells the Mage Guild holds for each of the five spell levels.</param>
    /// <param name="MageGuildSpells">
    /// The spells of the Mage Guild for each of the five spell levels, in the order the guild shows them: as many as
    /// <paramref name="SpellCounts"/> says, and for a guild that is not built yet the five, four, three, two and one it gets when built.
    /// A spell the editor does not know, and a level the faction cannot build, are left out. The game picks the spells when the map starts.
    /// </param>
    /// <param name="EnabledBuildings">The buildings the map allows to build.</param>
    /// <param name="BuiltThisTurn">Whether something has been built in the castle this turn.</param>
    /// <param name="MageGuildLevel">The level of the Mage Guild, 0 when there is none.</param>
    /// <param name="CreaturesAvailable">
    /// The creatures waiting in the dwellings, seven counts for the creatures of level 1 to 7. Once a dwelling is upgraded
    /// its count is in <paramref name="UpgradedCreaturesAvailable"/>.
    /// </param>
    /// <param name="UpgradedCreaturesAvailable">The same for the upgraded dwellings.</param>
    public sealed record CastleModel(
        string Name,
        Faction? Faction,
        PlayerColor? Owner,
        Position Position,
        Army Garrison,
        IReadOnlySet<Building> Buildings,
        Position? ShipyardLocation,
        int? GarrisonedHeroId,
        int? VisitingHeroId,
        IReadOnlySet<Building> EnabledBuildings,
        bool BuiltThisTurn,
        int MageGuildLevel,
        IReadOnlyList<int> CreaturesAvailable,
        IReadOnlyList<int> UpgradedCreaturesAvailable,
        IReadOnlySet<Building> CurrentBuildings,
        IReadOnlyList<int> SpellCounts,
        IReadOnlyList<IReadOnlyList<Spell>> MageGuildSpells)
    {
        /// <summary>
        /// Returns whether the castle has built <paramref name="building"/>, or a building that replaced it.
        /// </summary>
        public bool HasBuilt(Building building) => this.Buildings.Contains(building);

        /// <summary>
        /// Returns the castle with <paramref name="garrison"/> as its own creatures. A garrisoned hero's army takes the place of the castle's in
        /// the game, so the slots of the castle cannot be set while a hero is garrisoned.
        /// </summary>
        /// <exception cref="GameRuleException">A hero is garrisoned in the castle.</exception>
        public CastleModel WithGarrison(Army garrison) => this.GarrisonedHeroId != null
            ? throw new GameRuleException("A hero is garrisoned here, so its army takes the place of the town's creatures.")
            : this with { Garrison = garrison };

        /// <summary>
        /// Returns the castle with <paramref name="spell"/> in a slot of the Mage Guild, whatever the spell was there. It does not
        /// check the rules of the game (see <see cref="CastleBuilder.SetMageGuildSpell"/>), only that the slot exists.
        /// </summary>
        /// <param name="level">The spell level, 1 to 5.</param>
        /// <param name="slot">The place in the list of the level, from 0.</param>
        public CastleModel WithMageGuildSpell(int level, int slot, Spell spell)
        {
            RangeGuard.ThrowIfOutOfRange(level, 1, this.MageGuildSpells.Count, nameof(level));
            RangeGuard.ThrowIfOutOfRange(slot, 0, this.MageGuildSpells[level - 1].Count - 1, nameof(slot));

            var levels = this.MageGuildSpells.Select(l => l.ToList()).ToList();
            levels[level - 1][slot] = spell;
            return this with { MageGuildSpells = levels.Select(l => (IReadOnlyList<Spell>)l.AsReadOnly()).ToList() };
        }
    }
}
