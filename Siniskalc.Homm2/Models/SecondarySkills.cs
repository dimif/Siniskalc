namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The secondary skills of a hero, in the order it learned them (a hero has at most eight). Compares by what it holds.
    /// </summary>
    public sealed class SecondarySkills : IEquatable<SecondarySkills>
    {
        private readonly SkillEntry[] entries;

        public SecondarySkills(IEnumerable<SkillEntry> entries)
        {
            this.entries = entries.Where(e => e.Level != SkillLevel.None).ToArray();
            RangeGuard.ThrowIfOutOfRange(this.entries.Length, 0, Constants.Hero.MaxSecondarySkills, nameof(entries));
            if (this.entries.Select(e => e.Skill).Distinct().Count() != this.entries.Length)
            {
                throw new ArgumentException("A skill can be listed once.", nameof(entries));
            }
        }

        public static SecondarySkills None { get; } = new([]);

        /// <summary>
        /// Gets the skills the hero has, in the order it learned them.
        /// </summary>
        public IReadOnlyList<SkillEntry> Entries => this.entries;

        public int Count => this.entries.Length;

        public bool IsFull => this.entries.Length >= Constants.Hero.MaxSecondarySkills;

        public SkillLevel LevelOf(SecondarySkill skill) => this.entries.FirstOrDefault(e => e.Skill == skill)?.Level ?? SkillLevel.None;

        /// <summary>
        /// Returns the skills with <paramref name="skill"/> at <paramref name="level"/>: learned last when the hero did not have it, changed
        /// where it is when it did, and taken away (the ones after it move up) at <see cref="SkillLevel.None"/>.
        /// </summary>
        /// <exception cref="GameRuleException">The hero has eight skills and <paramref name="skill"/> is none of them.</exception>
        public SecondarySkills With(SecondarySkill skill, SkillLevel level)
        {
            var known = this.entries.Any(e => e.Skill == skill);
            if (!known)
            {
                if (level == SkillLevel.None)
                {
                    return this;
                }

                return this.IsFull
                    ? throw new GameRuleException("A hero has at most eight secondary skills.")
                    : new SecondarySkills(this.entries.Append(new SkillEntry(skill, level)));
            }

            return new SecondarySkills(this.entries.Select(e => e.Skill == skill ? e with { Level = level } : e));
        }

        /// <summary>
        /// Returns the skills with the ones in two places swapped: the order is the order they were learned, which the hero screen shows.
        /// </summary>
        public SecondarySkills Swap(int first, int second)
        {
            RangeGuard.ThrowIfNotIndex(first, this.entries.Length, nameof(first));
            RangeGuard.ThrowIfNotIndex(second, this.entries.Length, nameof(second));
            var copy = (SkillEntry[])this.entries.Clone();
            (copy[first], copy[second]) = (copy[second], copy[first]);
            return new SecondarySkills(copy);
        }

        /// <summary>
        /// Returns the skills without the one in a place; the ones after it move up.
        /// </summary>
        public SecondarySkills WithoutSlot(int slot)
        {
            RangeGuard.ThrowIfNotIndex(slot, this.entries.Length, nameof(slot));
            return new SecondarySkills(this.entries.Where((_, i) => i != slot));
        }

        /// <summary>
        /// Returns the skills with another skill in a place, at the level the place had. Nothing changes when the hero has the skill already.
        /// </summary>
        public SecondarySkills WithSkillInSlot(int slot, SecondarySkill skill)
        {
            RangeGuard.ThrowIfNotIndex(slot, this.entries.Length, nameof(slot));
            return this.entries.Any(e => e.Skill == skill) ? this : new SecondarySkills(this.entries.Select((e, i) => i == slot ? e with { Skill = skill } : e));
        }

        public bool Equals(SecondarySkills? other) => other is not null && this.entries.AsSpan().SequenceEqual(other.entries);

        public override bool Equals(object? obj) => this.Equals(obj as SecondarySkills);

        public override int GetHashCode()
        {
            var hash = default(HashCode);
            foreach (var entry in this.entries)
            {
                hash.Add(entry);
            }

            return hash.ToHashCode();
        }

        public override string ToString() => string.Join(", ", this.entries.Select(e => $"{e.Level} {e.Skill.Name}"));
    }
}
