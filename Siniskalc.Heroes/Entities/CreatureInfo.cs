namespace Siniskalc.Heroes.Entities
{
    using System.Collections.Generic;

    /// <summary>
    /// Combat stats, cost and abilities of a creature, in the form both games use. Heroes 3 values are as listed on the Heroes 3 wiki.
    /// The AI and Fight values (0 where the game has none or they are not known) were last fetched from https://heroes.thelazy.net/index.php/AI/Fight_Value on 2026-10-09.
    /// <see cref="Shots"/> is 0 for creatures that are not ranged.
    /// </summary>
    public readonly record struct CreatureInfo(short Atk, short Def, short MinDamage, short MaxDamage, short Health, short Speed, short Growth, int AiValue, int FightValue, int Gold, short Wood, short Ore, short Mercury, short Sulfur, short Crystal, short Gems, short Shots, IReadOnlyList<ICreatureAbility>? Special);
}
