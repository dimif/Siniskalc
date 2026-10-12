namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// What a slot of an army holds: the creature, how many, and what a stack of a size is worth (<c>null</c> when the game has no such thing).
    /// </summary>
    public sealed record StackContent(string CreatureName, int Amount, Func<int, IReadOnlyList<StackFact>>? FactsOf = null);
}
