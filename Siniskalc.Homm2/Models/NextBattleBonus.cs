namespace Siniskalc.Homm2.Models
{
    /// <summary>
    /// The morale and the luck that a hero has for its next battle only, from the places that give them (a temple or a fountain, the
    /// faerie ring, a graveyard that was visited before): the game takes them away after a battle. Each is a signed byte of the hero's record.
    /// </summary>
    /// <param name="Morale">The change of morale (a temple gives 2, a graveyard that was visited already -1).</param>
    /// <param name="Luck">The change of luck (a fountain or a faerie ring gives 1).</param>
    public readonly record struct NextBattleBonus(int Morale, int Luck);
}
