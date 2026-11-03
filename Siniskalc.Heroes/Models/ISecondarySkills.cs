namespace Siniskalc.Heroes.Models
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// Represents secondary skills of a hero.
    /// </summary>
    /// <typeparam name="TSecondarySkill">
    /// The type of secondary skill - game specific.
    /// </typeparam>
    /// <typeparam name="TSelf">
    /// The type of the implementing class, used for fluent methods.
    /// </typeparam>
    /// <remarks>
    /// CRTP.
    /// </remarks>
    public interface ISecondarySkills<TSecondarySkill, TSelf>
        where TSecondarySkill : ISecondarySkill
        where TSelf : ISecondarySkills<TSecondarySkill, TSelf>
    {
        /// <summary>
        /// Gets the number of slots occupied by skills.
        /// </summary>
        int SlotCount { get; }

        /// <summary>
        /// Gets a value indicating whether all visible skill slots are occupied.
        /// </summary>
        bool IsFull { get; }

        /// <summary>
        /// Returns the level of <paramref name="skill"/>, or <see cref="SkillLevel.None"/> when the hero does not have it.
        /// </summary>
        /// <param name="skill">
        /// The skill to look up.
        /// </param>
        /// <returns>
        /// The level of the skill, or <see cref="SkillLevel.None"/> when the hero does not have the skill.
        /// </returns>
        SkillLevel GetSkillLevel(TSecondarySkill skill);

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
        TSecondarySkill GetSkillInSlot(int slotIndex);

        /// <summary>
        /// Returns the slot containing <paramref name="skill"/>, or -1 when it is not in a slot.
        /// </summary>
        /// <param name="skill">
        /// The skill to look up.
        /// </param>
        /// <returns>
        /// The slot containing the skill, or -1 when the skill is not in the skillset.
        /// </returns>
        int GetSlotOf(TSecondarySkill skill);

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
        TSelf WithSkill(TSecondarySkill skill, SkillLevel level);

        /// <summary>
        /// Returns a new copy with the skill at <paramref name="level"/>, or without the skill when <paramref name="level"/> is
        /// <see cref="SkillLevel.None"/>. A hero whose slots are full does not learn a skill it has not got; the copy is the same.
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
        TSelf WithSkillLevel(TSecondarySkill skill, SkillLevel level);

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
        TSelf Swap(int slotIndex, int otherSlotIndex);

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
        TSelf WithoutSlot(int slotIndex);

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
        TSelf WithSkillInSlot(int slotIndex, TSecondarySkill skill);
    }
}
