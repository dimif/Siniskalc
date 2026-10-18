namespace Siniskalc.Homm3.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Immutable secondary skills of a hero: a level per skill, plus the order in which the skills are shown
    /// (the skill slots). Slots are always contiguous, so removing a skill moves the later ones up.
    /// The game only shows <see cref="Constants.SkillSlots.Max"/> slots, but a hero can have a level in any skill; skills with a level
    /// and no slot are "extra" skills, which the game still applies. Edits return a new instance.
    /// </summary>
    public sealed class SecondarySkills : ISecondarySkills<SecondarySkill, SecondarySkills>
    {
        private readonly IDictionary<SecondarySkill, SkillLevel> allSkillsToLevels;

        private readonly SecondarySkill[] skillSlots;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecondarySkills"/> class.
        /// </summary>
        /// <param name="allSkillsToLevels">
        /// The map of all of the skills of the hero to their levels.
        /// </param>
        /// <param name="skillSlots">
        /// The skills in the hero slots, in order. The length of this array is the number of occupied slots.
        /// </param>
        public SecondarySkills(IDictionary<SecondarySkill, SkillLevel> allSkillsToLevels, SecondarySkill[] skillSlots)
        {
            this.allSkillsToLevels = allSkillsToLevels;
            this.skillSlots = skillSlots;
        }

        /// <summary>
        /// Gets the number of slots occupied by skills.
        /// </summary>
        public int SlotCount => this.skillSlots.Length;

        /// <summary>
        /// Gets a value indicating whether all visible skill slots are occupied.
        /// </summary>
        public bool IsFull => this.skillSlots.Length >= Constants.SkillSlots.Max;

        /// <summary>
        /// Gets the skills that have a level but no slot, so the hero screen does not show them.
        /// </summary>
        public IEnumerable<SecondarySkill> ExtraSkills =>
            this.allSkillsToLevels.Where(pair => pair.Value != SkillLevel.None && !this.skillSlots.Contains(pair.Key)).Select(pair => pair.Key);

        /// <summary>
        /// Returns the level of <paramref name="skill"/>, or <see cref="SkillLevel.None"/> when the hero does not have it.
        /// </summary>
        /// <param name="skill">
        /// The skill to look up.
        /// </param>
        /// <returns>
        /// The level of the skill, or <see cref="SkillLevel.None"/> when the hero does not have the skill.
        /// </returns>
        public SkillLevel GetSkillLevel(SecondarySkill skill)
        {
            return this.allSkillsToLevels.TryGetValue(skill, out var level) ? level : SkillLevel.None;
        }

        /// <summary>
        /// Returns the skill in <paramref name="slotIndex"/>.
        /// </summary>
        /// <param name="slotIndex">
        /// The slot.
        /// </param>
        /// <returns>
        /// The skill in the slot.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the index is outside the occupied slots.
        /// </exception>
        public SecondarySkill GetSkillInSlot(int slotIndex)
        {
            RangeGuard.ThrowIfNotIndex(slotIndex, this.skillSlots.Length, nameof(slotIndex), "Slot index is out of range.");

            return this.skillSlots[slotIndex];
        }

        /// <summary>
        /// Returns the slot containing <paramref name="skill"/>, or -1 when it is not in a slot.
        /// </summary>
        public int GetSlotOf(SecondarySkill skill)
        {
            return Array.IndexOf(this.skillSlots, skill);
        }

        /// <summary>
        /// Returns a new copy of the secondary skills with the provided skill at the provided level.
        /// </summary>
        /// <param name="skill">
        /// The skill.
        /// </param>
        /// <param name="level">
        /// The level.
        /// </param>
        /// <returns>
        /// A new, updated, copy of the secondary skills.
        /// </returns>
        public SecondarySkills WithSkill(SecondarySkill skill, SkillLevel level)
        {
            var slots = this.skillSlots;

            if (!slots.Contains(skill) && slots.Length < Constants.SkillSlots.Max && !this.IsExtra(skill))
            {
                slots = [.. slots, skill];
            }

            var levels = new Dictionary<SecondarySkill, SkillLevel>(this.allSkillsToLevels) { [skill] = level };
            return new SecondarySkills(levels, slots);
        }

        /// <summary>
        /// Returns a copy with the skill at <paramref name="level"/>, or removed altogether when <paramref name="level"/> is <see cref="SkillLevel.None"/>.
        /// Removing a skill that has a slot moves the later skills up.
        /// </summary>
        public SecondarySkills WithSkillLevel(SecondarySkill skill, SkillLevel level)
        {
            if (level != SkillLevel.None)
            {
                return this.WithSkill(skill, level);
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

            return new SecondarySkills(new Dictionary<SecondarySkill, SkillLevel>(this.allSkillsToLevels) { [skill] = SkillLevel.None }, this.skillSlots);
        }

        /// <summary>
        /// Returns a new copy of the secondary skills where a slot is inserted at the given index, replacing the one existing.
        /// The skill level is therefore the same as the one of the replaced skill.
        /// </summary>
        /// <param name="slotIndex">
        /// The slot.
        /// </param>
        /// <param name="skill">
        /// The skill.
        /// </param>
        /// <returns>
        /// A new, updated, copy of the secondary skills.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the index is outside the allowed range.
        /// </exception>
        public SecondarySkills WithSkillInSlot(int slotIndex, SecondarySkill skill)
        {
            RangeGuard.ThrowIfNotIndex(slotIndex, this.skillSlots.Length, nameof(slotIndex));

            var current = this.skillSlots[slotIndex];
            if (this.skillSlots.Contains(skill))
            {
                return this;
            }

            var levels = new Dictionary<SecondarySkill, SkillLevel>(this.allSkillsToLevels)
            {
                [current] = SkillLevel.None,
                [skill] = this.LevelToKeep(skill, current),
            };

            var slots = (SecondarySkill[])this.skillSlots.Clone();
            slots[slotIndex] = skill;
            return new SecondarySkills(levels, slots);
        }

        /// <summary>
        /// Returns a new copy without the skill in <paramref name="slotIndex"/>; later skills move up a slot.
        /// </summary>
        /// <param name="slotIndex">
        /// The slot.
        /// </param>
        /// <returns>
        /// A new copy without the skill in the slot.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the index is outside the occupied slots.
        /// </exception>
        public SecondarySkills WithoutSlot(int slotIndex)
        {
            RangeGuard.ThrowIfNotIndex(slotIndex, this.skillSlots.Length, nameof(slotIndex));

            var removed = this.skillSlots[slotIndex];
            var levels = new Dictionary<SecondarySkill, SkillLevel>(this.allSkillsToLevels) { [removed] = SkillLevel.None };
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

            return new SecondarySkills(new Dictionary<SecondarySkill, SkillLevel>(this.allSkillsToLevels), [.. this.skillSlots, skill]);
        }

        /// <summary>
        /// Returns a copy where the skill in <paramref name="slotIndex"/> keeps its level but leaves the slot, so the
        /// hero screen no longer shows it. Later skills move up a slot.
        /// </summary>
        public SecondarySkills WithSlotAsExtra(int slotIndex)
        {
            RangeGuard.ThrowIfNotIndex(slotIndex, this.skillSlots.Length, nameof(slotIndex));

            var slots = this.skillSlots.Where((_, index) => index != slotIndex).ToArray();
            return new SecondarySkills(new Dictionary<SecondarySkill, SkillLevel>(this.allSkillsToLevels), slots);
        }

        /// <summary>
        /// Returns a new copy of the secondary skills where 2 slots have been swapped.
        /// </summary>
        /// <param name="slotIndex">
        /// First index to swap.
        /// </param>
        /// <param name="otherSlotIndex">
        /// Second index to swap.
        /// </param>
        /// <returns>
        /// A new, updated, copy of the secondary skills.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either of the indexes is outside the allowed range.
        /// </exception>
        public SecondarySkills Swap(int slotIndex, int otherSlotIndex)
        {
            RangeGuard.ThrowIfNotIndex(slotIndex, this.skillSlots.Length, nameof(slotIndex));
            RangeGuard.ThrowIfNotIndex(otherSlotIndex, this.skillSlots.Length, nameof(otherSlotIndex));

            var slots = (SecondarySkill[])this.skillSlots.Clone();

            (slots[slotIndex], slots[otherSlotIndex]) = (slots[otherSlotIndex], slots[slotIndex]);
            return new SecondarySkills(new Dictionary<SecondarySkill, SkillLevel>(this.allSkillsToLevels), slots);
        }

        private SkillLevel LevelToKeep(SecondarySkill skill, SecondarySkill replaced)
        {
            var own = this.GetSkillLevel(skill);
            return own != SkillLevel.None ? own : this.GetSkillLevel(replaced) != SkillLevel.None ? this.GetSkillLevel(replaced) : SkillLevel.Beginner;
        }

        private bool IsExtra(SecondarySkill skill)
        {
            return this.GetSkillLevel(skill) != SkillLevel.None && !this.skillSlots.Contains(skill);
        }
    }
}
