namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// A page of secondary skills over a hero made by the test: skills in contiguous slots, extra skills the game applies but does not
    /// show, and a note of everything the base class asks of the game.
    /// </summary>
    internal sealed class TestSkills : SecondarySkillsViewModelBase
    {
        public TestSkills(int maxSlots = 3, bool supportsHiding = false)
            : base(["Archery", "Luck", "Wisdom", "Tactics", "Scouting"], maxSlots)
        {
            this.Hiding = supportsHiding;
        }

        public List<(string Skill, string Level)> InSlots { get; } = [];

        public Dictionary<string, string> Extras { get; } = [];

        public List<string> Calls { get; } = [];

        public int Extra { get; set; }

        /// <summary>What the game does when it is asked for the level of a slot, as a game that edits the page while it is refreshed would.</summary>
        public Action? OnLevelRead { get; set; }

        public override bool SupportsHiding => this.Hiding;

        public override int ExtraCount => this.Extra;

        protected override int UsedSlots => this.InSlots.Count;

        private bool Hiding { get; }

        protected override string? SkillInSlot(int slot) => this.InSlots[slot].Skill;

        protected override string LevelInSlot(int slot)
        {
            this.OnLevelRead?.Invoke();
            return this.InSlots[slot].Level;
        }

        protected override string LevelOf(string skillName)
        {
            var slot = this.SlotOf(skillName);
            return slot >= 0 ? this.InSlots[slot].Level : this.Extras.GetValueOrDefault(skillName, SkillLevelNames.None);
        }

        protected override int SlotOf(string skillName) => this.InSlots.FindIndex(s => s.Skill == skillName);

        protected override void ReplaceSkillInSlot(int slot, string skillName)
        {
            this.Calls.Add($"replace {slot} {skillName}");
            this.InSlots[slot] = (skillName, this.InSlots[slot].Level);
        }

        protected override void AddSkill(string skillName)
        {
            this.Calls.Add($"add {skillName}");
            this.InSlots.Add((skillName, SkillLevelNames.Basic));
        }

        protected override void SetLevelInSlot(int slot, string levelName)
        {
            this.Calls.Add($"level in slot {slot} {levelName}");
            this.InSlots[slot] = (this.InSlots[slot].Skill, levelName);
        }

        protected override void SetLevelOf(string skillName, string levelName)
        {
            this.Calls.Add($"level of {skillName} {levelName}");
            var slot = this.SlotOf(skillName);
            if (slot >= 0)
            {
                this.InSlots[slot] = (skillName, levelName);
            }
            else
            {
                this.Extras[skillName] = levelName;
            }
        }

        protected override void SwapSlots(int first, int second)
        {
            this.Calls.Add($"swap {first} {second}");
            (this.InSlots[first], this.InSlots[second]) = (this.InSlots[second], this.InSlots[first]);
        }

        protected override void RemoveSlot(int slot)
        {
            this.Calls.Add($"remove {slot}");
            this.InSlots.RemoveAt(slot);
        }

        protected override void HideSlot(int slot)
        {
            this.Calls.Add($"hide {slot}");
            this.Extras[this.InSlots[slot].Skill] = this.InSlots[slot].Level;
            this.InSlots.RemoveAt(slot);
        }

        protected override void ShowExtraSkill(string skillName)
        {
            this.Calls.Add($"show {skillName}");
            this.InSlots.Add((skillName, this.Extras[skillName]));
            this.Extras.Remove(skillName);
        }
    }
}
