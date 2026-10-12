namespace Siniskalc.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Entities;

    /// <summary>
    /// Immutable secondary skills of a hero: a level per skill, plus the order in which the skills are shown
    /// (the skill slots). Slots are always contiguous, so removing a skill moves the later ones up.
    /// The game only shows <see cref="Constants.SkillSlots.Max"/> slots, but a hero can have a level in any skill; skills with a level
    /// and no slot are "extra" skills, which the game still applies. Edits return a new instance.
    /// </summary>
    public sealed class SecondarySkills
    {
        private readonly IDictionary<SecondarySkill, SkillLevel?> allSkillsToLevels;

        private readonly SecondarySkill[] skillSlots;

        public SecondarySkills(IDictionary<SecondarySkill, SkillLevel?> allSkillsToLevels, SecondarySkill[] skillSlots)
        {
            this.allSkillsToLevels = allSkillsToLevels;
            this.skillSlots = skillSlots;
        }

        public int SlotCount => this.skillSlots.Length;

        /// <summary>
        /// Gets the skills that have a level but no slot, so the hero screen does not show them.
        /// </summary>
        public IEnumerable<SecondarySkill> ExtraSkills =>
            this.allSkillsToLevels.Where(pair => pair.Value.HasValue && !this.skillSlots.Contains(pair.Key)).Select(pair => pair.Key);

        public SkillLevel? GetSkillLevel(SecondarySkill skill)
        {
            return this.allSkillsToLevels.TryGetValue(skill, out var level) ? level : null;
        }

        public SecondarySkill? GetSkillInSlot(int slotIndex)
        {
            RangeGuard.ThrowIfNotIndex(slotIndex, this.skillSlots.Length, nameof(slotIndex), "Slot index is out of range.");

            return this.skillSlots[slotIndex];
        }

        /// <summary>
        /// Returns the slot (from 0) the skill is in, or -1 if it has none.
        /// </summary>
        public int GetSlotOf(SecondarySkill skill)
        {
            return Array.IndexOf(this.skillSlots, skill);
        }

        /// <summary>
        /// Returns a copy with <paramref name="skill"/> at <paramref name="level"/>. A skill that is not in a slot yet
        /// goes in the first free slot; when all <see cref="Constants.SkillSlots.Max"/> are taken it becomes an extra skill.
        /// </summary>
        public SecondarySkills WithSkill(SecondarySkill skill, SkillLevel level)
        {
            var slots = this.skillSlots;

            if (!slots.Contains(skill) && slots.Length < Constants.SkillSlots.Max && !this.IsExtra(skill))
            {
                slots = [.. slots, skill];
            }

            var levels = new Dictionary<SecondarySkill, SkillLevel?>(this.allSkillsToLevels) { [skill] = level };
            return new SecondarySkills(levels, slots);
        }

        /// <summary>
        /// Returns a copy with the skill at <paramref name="level"/>, or removed altogether when <paramref name="level"/> is <c>null</c>.
        /// Removing a skill that has a slot moves the later skills up.
        /// </summary>
        public SecondarySkills WithSkillLevel(SecondarySkill skill, SkillLevel? level)
        {
            if (level.HasValue)
            {
                return this.WithSkill(skill, level.Value);
            }

            var slot = this.GetSlotOf(skill);
            if (slot >= 0)
            {
                return this.WithoutSlot(slot);
            }

            if (!this.IsExtra(skill))
            {
                return this;
            }

            return new SecondarySkills(new Dictionary<SecondarySkill, SkillLevel?>(this.allSkillsToLevels) { [skill] = null }, this.skillSlots);
        }

        /// <summary>
        /// Returns a copy with the skill in <paramref name="slotIndex"/> replaced by <paramref name="skill"/>.
        /// The replaced skill is removed. The new skill keeps its level when it already has one (an extra skill),
        /// and otherwise takes over the level of the skill it replaces. Ignored when the skill is already in a slot.
        /// </summary>
        public SecondarySkills WithSkillInSlot(int slotIndex, SecondarySkill skill)
        {
            this.ThrowIfOutOfRange(slotIndex);

            var current = this.skillSlots[slotIndex];
            if (this.skillSlots.Contains(skill))
            {
                return this;
            }

            var levels = new Dictionary<SecondarySkill, SkillLevel?>(this.allSkillsToLevels)
            {
                [current] = null,
                [skill] = this.GetSkillLevel(skill) ?? this.GetSkillLevel(current) ?? SkillLevel.Beginner,
            };

            var slots = (SecondarySkill[])this.skillSlots.Clone();
            slots[slotIndex] = skill;
            return new SecondarySkills(levels, slots);
        }

        /// <summary>
        /// Returns a copy without the skill in <paramref name="slotIndex"/>; later skills move up a slot.
        /// </summary>
        public SecondarySkills WithoutSlot(int slotIndex)
        {
            this.ThrowIfOutOfRange(slotIndex);

            var removed = this.skillSlots[slotIndex];
            var levels = new Dictionary<SecondarySkill, SkillLevel?>(this.allSkillsToLevels) { [removed] = null };
            var slots = this.skillSlots.Where((_, index) => index != slotIndex).ToArray();
            return new SecondarySkills(levels, slots);
        }

        /// <summary>
        /// Returns a copy with an extra skill moved into the first free slot, so the hero screen shows it.
        /// Ignored when the skill is not an extra skill or all <see cref="Constants.SkillSlots.Max"/> slots are taken.
        /// </summary>
        public SecondarySkills WithExtraSkillInSlot(SecondarySkill skill)
        {
            if (!this.IsExtra(skill) || this.skillSlots.Length >= Constants.SkillSlots.Max)
            {
                return this;
            }

            return new SecondarySkills(new Dictionary<SecondarySkill, SkillLevel?>(this.allSkillsToLevels), [.. this.skillSlots, skill]);
        }

        /// <summary>
        /// Returns a copy where the skill in <paramref name="slotIndex"/> keeps its level but leaves the slot, so the
        /// hero screen no longer shows it. Later skills move up a slot.
        /// </summary>
        public SecondarySkills WithSlotAsExtra(int slotIndex)
        {
            this.ThrowIfOutOfRange(slotIndex);

            var slots = this.skillSlots.Where((_, index) => index != slotIndex).ToArray();
            return new SecondarySkills(new Dictionary<SecondarySkill, SkillLevel?>(this.allSkillsToLevels), slots);
        }

        /// <summary>
        /// Returns a copy with the skills in the two slots swapped.
        /// </summary>
        public SecondarySkills Swap(int slotIndex, int otherSlotIndex)
        {
            this.ThrowIfOutOfRange(slotIndex);
            this.ThrowIfOutOfRange(otherSlotIndex);

            var slots = (SecondarySkill[])this.skillSlots.Clone();
            (slots[slotIndex], slots[otherSlotIndex]) = (slots[otherSlotIndex], slots[slotIndex]);
            return new SecondarySkills(new Dictionary<SecondarySkill, SkillLevel?>(this.allSkillsToLevels), slots);
        }

        private bool IsExtra(SecondarySkill skill)
        {
            return this.GetSkillLevel(skill).HasValue && !this.skillSlots.Contains(skill);
        }

        private void ThrowIfOutOfRange(int slotIndex)
        {
            RangeGuard.ThrowIfNotIndex(slotIndex, this.skillSlots.Length, nameof(slotIndex), "Slot index is out of range.");
        }
    }
}
