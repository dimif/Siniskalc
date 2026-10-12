namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The secondary skills of a hero, in the order it learned them (a hero has at most eight). Compares by what it holds.
    /// </summary>
    public sealed class Homm2SecondarySkills : IEquatable<Homm2SecondarySkills>
    {
        private readonly Homm2SkillEntry[] entries;

        public Homm2SecondarySkills(IEnumerable<Homm2SkillEntry> entries)
        {
            this.entries = entries.Where(e => e.Level != Homm2SkillLevel.None).ToArray();
            RangeGuard.ThrowIfOutOfRange(this.entries.Length, 0, Constants.Hero.MaxSecondarySkills, nameof(entries));
            if (this.entries.Select(e => e.Skill).Distinct().Count() != this.entries.Length)
            {
                throw new ArgumentException("A skill can be listed once.", nameof(entries));
            }
        }

        public static Homm2SecondarySkills None { get; } = new([]);

        /// <summary>
        /// Gets the skills the hero has, in the order it learned them.
        /// </summary>
        public IReadOnlyList<Homm2SkillEntry> Entries => this.entries;

        public int Count => this.entries.Length;

        public bool IsFull => this.entries.Length >= Constants.Hero.MaxSecondarySkills;

        public Homm2SkillLevel LevelOf(Homm2SecondarySkill skill) => this.entries.FirstOrDefault(e => e.Skill == skill)?.Level ?? Homm2SkillLevel.None;

        /// <summary>
        /// Returns the skills with <paramref name="skill"/> at <paramref name="level"/>: learned last when the hero did not have it, changed
        /// where it is when it did, and taken away (the ones after it move up) at <see cref="Homm2SkillLevel.None"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">The hero has eight skills and <paramref name="skill"/> is none of them.</exception>
        public Homm2SecondarySkills With(Homm2SecondarySkill skill, Homm2SkillLevel level)
        {
            var known = this.entries.Any(e => e.Skill == skill);
            if (!known)
            {
                if (level == Homm2SkillLevel.None)
                {
                    return this;
                }

                return this.IsFull
                    ? throw new InvalidOperationException("A hero has at most eight secondary skills.")
                    : new Homm2SecondarySkills(this.entries.Append(new Homm2SkillEntry(skill, level)));
            }

            return new Homm2SecondarySkills(this.entries.Select(e => e.Skill == skill ? e with { Level = level } : e));
        }

        /// <summary>
        /// Returns the skills with the ones in two places swapped: the order is the order they were learned, which the hero screen shows.
        /// </summary>
        public Homm2SecondarySkills Swap(int first, int second)
        {
            RangeGuard.ThrowIfNotIndex(first, this.entries.Length, nameof(first));
            RangeGuard.ThrowIfNotIndex(second, this.entries.Length, nameof(second));
            var copy = (Homm2SkillEntry[])this.entries.Clone();
            (copy[first], copy[second]) = (copy[second], copy[first]);
            return new Homm2SecondarySkills(copy);
        }

        /// <summary>
        /// Returns the skills without the one in a place; the ones after it move up.
        /// </summary>
        public Homm2SecondarySkills WithoutSlot(int slot)
        {
            RangeGuard.ThrowIfNotIndex(slot, this.entries.Length, nameof(slot));
            return new Homm2SecondarySkills(this.entries.Where((_, i) => i != slot));
        }

        /// <summary>
        /// Returns the skills with another skill in a place, at the level the place had. Nothing changes when the hero has the skill already.
        /// </summary>
        public Homm2SecondarySkills WithSkillInSlot(int slot, Homm2SecondarySkill skill)
        {
            RangeGuard.ThrowIfNotIndex(slot, this.entries.Length, nameof(slot));
            return this.entries.Any(e => e.Skill == skill) ? this : new Homm2SecondarySkills(this.entries.Select((e, i) => i == slot ? e with { Skill = skill } : e));
        }

        public bool Equals(Homm2SecondarySkills? other) => other is not null && this.entries.AsSpan().SequenceEqual(other.entries);

        public override bool Equals(object? obj) => this.Equals(obj as Homm2SecondarySkills);

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
