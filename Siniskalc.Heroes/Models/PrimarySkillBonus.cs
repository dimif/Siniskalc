namespace Siniskalc.Heroes.Models
{
    /// <summary>
    /// What artifacts add to the four primary skills (less than nothing for an artifact that takes away). It is worked out from the
    /// artifacts a hero wears and is never saved on its own.
    /// </summary>
    public readonly record struct PrimarySkillBonus(int Attack, int Defense, int SpellPower, int Knowledge)
    {
        /// <summary>
        /// Gets the bonus of none.
        /// </summary>
        public static PrimarySkillBonus None => default;

        /// <summary>
        /// Returns the bonus of two sets of artifacts together.
        /// </summary>
        public static PrimarySkillBonus operator +(PrimarySkillBonus left, PrimarySkillBonus right) =>
            new(left.Attack + right.Attack, left.Defense + right.Defense, left.SpellPower + right.SpellPower, left.Knowledge + right.Knowledge);

        /// <summary>
        /// Returns what is left of a bonus when another one is taken from it.
        /// </summary>
        public static PrimarySkillBonus operator -(PrimarySkillBonus left, PrimarySkillBonus right) =>
            new(left.Attack - right.Attack, left.Defense - right.Defense, left.SpellPower - right.SpellPower, left.Knowledge - right.Knowledge);
    }
}
