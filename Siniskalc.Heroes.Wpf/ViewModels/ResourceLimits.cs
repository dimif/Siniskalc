namespace Siniskalc.Heroes.Wpf.ViewModels
{
    /// <summary>
    /// What the editor lets a player hold of each resource and by how much its steppers and presets move it: gold has limits of its own.
    /// A game gives its values (they are kept in its <c>Constants</c>).
    /// </summary>
    /// <param name="Minimum">The least of any resource.</param>
    /// <param name="MaxGold">The most gold.</param>
    /// <param name="MaxOther">The most of any other resource.</param>
    /// <param name="GoldStep">What the stepper of gold moves by.</param>
    /// <param name="OtherStep">What the stepper of any other resource moves by.</param>
    /// <param name="AllIncrease">What the "all resources" button adds to every resource but gold.</param>
    /// <param name="GoldIncrease">What the "rich" button adds to gold.</param>
    /// <param name="OtherPreset">What the button beside each resource but gold adds to it.</param>
    /// <param name="GoldPreset">What the button beside gold adds to it.</param>
    public sealed record ResourceLimits(long Minimum, long MaxGold, long MaxOther, long GoldStep, long OtherStep, int AllIncrease, int GoldIncrease, int OtherPreset, int GoldPreset);
}
