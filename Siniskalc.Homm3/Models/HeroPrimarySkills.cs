namespace Siniskalc.Homm3.Models
{
    using System;
    using Siniskalc.Core;

    /// <summary>
    /// The four primary skills of a hero. <see cref="Atk"/>, <see cref="Def"/>, <see cref="Pow"/> and <see cref="Kno"/> are the values
    /// the savefile holds, which are what counts in the game: the hero's own skill with what the equipped artifacts add already in it
    /// (a hero with Armageddon's Blade is saved with 3 more attack). <see cref="Bonus"/> is that part, kept up to date by the hero, and
    /// the <c>Base</c> properties are what is left without it, which is what the hero learned and what a level-up raises.
    /// </summary>
    public record HeroPrimarySkills(byte Atk, byte Def, byte Pow, byte Kno)
    {
        public PrimarySkillBonus Bonus { get; init; }

        public int BaseAtk => Base(this.Atk, this.Bonus.Atk);

        public int BaseDef => Base(this.Def, this.Bonus.Def);

        public int BasePow => Base(this.Pow, this.Bonus.Pow);

        public int BaseKno => Base(this.Kno, this.Bonus.Kno);

        /// <summary>
        /// Two skill sets are equal when the values the savefile holds are: the bonus comes from the artifacts and is worked out again
        /// whenever they change, so it is not part of what a hero's skills are.
        /// </summary>
        public virtual bool Equals(HeroPrimarySkills? other)
        {
            return other is not null && this.Atk == other.Atk && this.Def == other.Def && this.Pow == other.Pow && this.Kno == other.Kno;
        }

        public override int GetHashCode() => HashCode.Combine(this.Atk, this.Def, this.Pow, this.Kno);

        /// <summary>
        /// Returns the skills with the base attack set to <paramref name="value"/>: what the artifacts add stays on top of it.
        /// </summary>
        public HeroPrimarySkills WithBaseAtk(int value) => this with { Atk = Saved(value, this.Bonus.Atk) };

        public HeroPrimarySkills WithBaseDef(int value) => this with { Def = Saved(value, this.Bonus.Def) };

        public HeroPrimarySkills WithBasePow(int value) => this with { Pow = Saved(value, this.Bonus.Pow) };

        public HeroPrimarySkills WithBaseKno(int value) => this with { Kno = Saved(value, this.Bonus.Kno) };

        /// <summary>
        /// Returns the skills as they are when the artifacts add <paramref name="to"/> where they added <see cref="Bonus"/>: what the game
        /// does when a hero puts an artifact on or takes it off. A skill stays within what the savefile can hold.
        /// </summary>
        public HeroPrimarySkills WithBonus(PrimarySkillBonus to) => new HeroPrimarySkills(
            Saved(this.Atk, to.Atk - this.Bonus.Atk),
            Saved(this.Def, to.Def - this.Bonus.Def),
            Saved(this.Pow, to.Pow - this.Bonus.Pow),
            Saved(this.Kno, to.Kno - this.Bonus.Kno))
        {
            Bonus = to,
        };

        private static int Base(byte saved, short bonus) => Math.Max(0, saved - bonus);

        private static byte Saved(int value, int add) => (byte)RangeGuard.Clamp<long>(value + add, Constants.Hero.MinPrimarySkill, Constants.Hero.MaxSavedPrimarySkill);
    }
}
