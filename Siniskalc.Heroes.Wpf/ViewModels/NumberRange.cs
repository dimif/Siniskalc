namespace Siniskalc.Heroes.Wpf.ViewModels
{
    /// <summary>
    /// The lowest and the highest a number can be and the step of a stepper that changes it. A game passes its own limits (from its <c>Constants</c>) to a card.
    /// </summary>
    /// <param name="Minimum">The lowest value.</param>
    /// <param name="Maximum">The highest value.</param>
    /// <param name="Step">How much a press of the plus or minus button changes the value.</param>
    public sealed record NumberRange(long Minimum, long Maximum, long Step = 1);
}
