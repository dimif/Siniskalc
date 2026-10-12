namespace Siniskalc.Models
{
    using System;

    /// <summary>
    /// The four primary skills of a hero. <see cref="Atk"/>, <see cref="Def"/>, <see cref="Pow"/> and <see cref="Kno"/> are the
    /// base values the savefile holds; <see cref="Bonus"/> is what artifacts add, kept up to date by the hero, and the
    /// <c>Total</c> properties are what counts in the game. Only the base values are saved.
    /// </summary>
    public record HeroPrimarySkills(byte Atk, byte Def, byte Pow, byte Kno)
    {
        public PrimarySkillBonus Bonus { get; init; }

        public int TotalAtk => Total(this.Atk, this.Bonus.Atk);

        public int TotalDef => Total(this.Def, this.Bonus.Def);

        public int TotalPow => Total(this.Pow, this.Bonus.Pow);

        public int TotalKno => Total(this.Kno, this.Bonus.Kno);

        /// <summary>
        /// Two skill sets are equal when their base values are: the bonus comes from the artifacts and is worked out again whenever
        /// they change, so it is not part of what a hero's skills are.
        /// </summary>
        public virtual bool Equals(HeroPrimarySkills? other)
        {
            return other is not null && this.Atk == other.Atk && this.Def == other.Def && this.Pow == other.Pow && this.Kno == other.Kno;
        }

        public override int GetHashCode() => HashCode.Combine(this.Atk, this.Def, this.Pow, this.Kno);

        private static int Total(byte baseValue, short bonus) => Math.Max(0, baseValue + bonus);
    }
}
