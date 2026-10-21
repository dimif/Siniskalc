namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The secondary skills of a hero, in the order it learned them (a hero has at most eight). Compares by what it holds.
    /// </summary>
    public sealed class SecondarySkills : ISecondarySkills<SecondarySkill, SecondarySkills>
    {
        private readonly SkillEntry[] entries;

        public static SecondarySkills None { get; } = new(Array.Empty<SkillEntry>());

        /// <summary>
        /// Initializes a new instance of the <see cref="SecondarySkills"/> class.
        /// </summary>
        /// <param name="entries">
        /// The skill entries.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the number of entries is outside the allowed range.
        /// </exception>
        public SecondarySkills(IEnumerable<SkillEntry> entries)
        {
            this.entries = entries.Where(e => e.Level != SkillLevel.None).ToArray();

            RangeGuard.ThrowIfOutOfRange(this.entries.Length, 0, Constants.Hero.MaxSecondarySkills, nameof(entries));

            if (this.entries.Select(e => e.Skill).Distinct().Count() != this.entries.Length)
            {
                throw new ArgumentException("A skill can be listed once.", nameof(entries));
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<SkillEntry> Entries => this.entries;

        /// <inheritdoc/>
        public int SlotCount => this.entries.Length;

        /// <inheritdoc/>
        public bool IsFull => this.entries.Length >= Constants.Hero.MaxSecondarySkills;

        /// <inheritdoc/>
        public SecondarySkill GetSkillInSlot(int slotIndex)
        {
            RangeGuard.ThrowIfNotIndex(slotIndex, this.entries.Length, nameof(slotIndex));
            return this.entries[slotIndex].Skill;
        }

        /// <inheritdoc/>
        public int GetSlotOf(SecondarySkill skill) => this.entries.ToList().FindIndex(entry => entry.Skill == skill);

        /// <inheritdoc/>
        public SkillLevel GetSkillLevel(SecondarySkill skill) => this.entries.FirstOrDefault(e => e.Skill == skill)?.Level ?? SkillLevel.None;

        /// <inheritdoc/>
        public SecondarySkills Swap(int slotIndex, int otherSlotIndex)
        {
            RangeGuard.ThrowIfNotIndex(slotIndex, this.entries.Length, nameof(slotIndex));
            RangeGuard.ThrowIfNotIndex(otherSlotIndex, this.entries.Length, nameof(otherSlotIndex));

            var copy = (SkillEntry[])this.entries.Clone();
            (copy[slotIndex], copy[otherSlotIndex]) = (copy[otherSlotIndex], copy[slotIndex]);

            return new SecondarySkills(copy);
        }

        /// <inheritdoc/>
        public SecondarySkills WithSkill(SecondarySkill skill, SkillLevel level)
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

        /// <inheritdoc/>
        public SecondarySkills WithSkillLevel(SecondarySkill skill, SkillLevel level)
        {
            var slot = this.GetSlotOf(skill);
            if (level == SkillLevel.None)
            {
                return slot < 0 ? this : this.WithoutSlot(slot);
            }

            return slot < 0 && this.IsFull ? this : this.WithSkill(skill, level);
        }

        /// <inheritdoc/>
        public SecondarySkills WithoutSlot(int slotIndex)
        {
            RangeGuard.ThrowIfNotIndex(slotIndex, this.entries.Length, nameof(slotIndex));

            return new SecondarySkills(this.entries.Where((_, i) => i != slotIndex));
        }

        /// <inheritdoc/>
        public SecondarySkills WithSkillInSlot(int slotIndex, SecondarySkill skill)
        {
            RangeGuard.ThrowIfNotIndex(slotIndex, this.entries.Length, nameof(slotIndex));

            return this.entries.Any(e => e.Skill == skill) ? this : new SecondarySkills(this.entries.Select((e, i) => i == slotIndex ? e with { Skill = skill } : e));
        }

        /// <inheritdoc/>
        public bool Equals(SecondarySkills? other) => other is not null && this.entries.AsSpan().SequenceEqual(other.entries);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => this.Equals(obj as SecondarySkills);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = default(HashCode);
            foreach (var entry in this.entries)
            {
                hash.Add(entry);
            }

            return hash.ToHashCode();
        }

        /// <inheritdoc/>
        public override string ToString() => string.Join(", ", this.entries.Select(e => $"{e.Level} {e.Skill.Name}"));

    }
}
