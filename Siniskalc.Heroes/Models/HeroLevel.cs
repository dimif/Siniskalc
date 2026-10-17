namespace Siniskalc.Heroes.Models
{
    /// <summary>
    /// The level of a hero and the experience it has: the two numbers the savefile holds and keeps in step.
    /// </summary>
    /// <param name="Experience">The experience points.</param>
    /// <param name="Level">The level, from 1.</param>
    public readonly record struct HeroLevel(long Experience, int Level);
}
