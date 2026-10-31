namespace Siniskalc.Heroes.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// The changes that every game's differs report in the same way: who owns a record, what is in the slots of an army, the primary and secondary skills, the experience, the place, the movement, the spell points and the learned spells of a hero, and the resources, the tavern and the countdown of a player.
    /// </summary>
    public static class ChangeSinkExtensions
    {
        private const string NoHero = "(none)";

        /// <summary>
        /// Adds the change of the owner of a record, if the owner differs. The text is "Neutral" or the name of the colour.
        /// </summary>
        /// <typeparam name="TModel">The model of the record.</typeparam>
        /// <typeparam name="TColor">The colour of the game.</typeparam>
        /// <param name="sink">Collects the change.</param>
        /// <param name="before">The owner as loaded.</param>
        /// <param name="after">The owner now.</param>
        /// <param name="withOwner">Returns the model with the given owner.</param>
        public static void AddOwner<TModel, TColor>(this ChangeSink<TModel> sink, TColor before, TColor after, Func<TModel, TColor, TModel> withOwner)
            where TColor : AbstractEnumeration, IPlayerColor
        {
            if (before == after)
            {
                return;
            }

            sink.Add(ChangeAreas.General, "Owner", OwnerNames<TColor>.Of(before), OwnerNames<TColor>.Of(after), model => withOwner(model, before));
        }

        /// <summary>
        /// Adds a change for each slot of an army that differs: "Slot n", with the stack as it was and as it is now.
        /// </summary>
        /// <typeparam name="TModel">The model of the record that holds the army.</typeparam>
        /// <typeparam name="TCreature">The creature of the game.</typeparam>
        /// <param name="sink">Collects the changes.</param>
        /// <param name="area">The area the changes are listed under, such as Creatures or Garrison.</param>
        /// <param name="before">The army as loaded.</param>
        /// <param name="after">The army now.</param>
        /// <param name="getArmy">Gets the army of a model.</param>
        /// <param name="withArmy">Returns the model with the given army.</param>
        public static void AddArmy<TModel, TCreature>(
            this ChangeSink<TModel> sink,
            ChangeArea area,
            IArmy<TCreature> before,
            IArmy<TCreature> after,
            Func<TModel, IArmy<TCreature>> getArmy,
            Func<TModel, IArmy<TCreature>, TModel> withArmy)
            where TCreature : class, IGameEntity
        {
            for (var slot = 0; slot < before.Slots.Count && slot < after.Slots.Count; slot++)
            {
                var (was, now, index) = (before.Slots[slot], after.Slots[slot], slot);
                if (was != now)
                {
                    sink.Add(area, $"Slot {slot + 1}", was?.ToString() ?? ChangeText.Empty, now?.ToString() ?? ChangeText.Empty, model => withArmy(model, getArmy(model).WithSlot(index, was)));
                }
            }
        }

        /// <summary>
        /// Adds a change for each primary skill that differs (attack, defense, spell power, knowledge), each put back on its own.
        /// </summary>
        /// <typeparam name="TModel">The model of the record that holds the skills.</typeparam>
        /// <param name="sink">Collects the changes.</param>
        /// <param name="before">The skills as loaded.</param>
        /// <param name="after">The skills now.</param>
        /// <param name="getSkills">Gets the skills of a model.</param>
        /// <param name="withSkills">Returns the model with the given skills.</param>
        public static void AddPrimarySkills<TModel>(
            this ChangeSink<TModel> sink,
            PrimarySkills before,
            PrimarySkills after,
            Func<TModel, PrimarySkills> getSkills,
            Func<TModel, PrimarySkills, TModel> withSkills)
        {
            if (before.Attack != after.Attack)
            {
                sink.Add(ChangeAreas.PrimarySkills, "Attack", before.Attack.ToString(), after.Attack.ToString(), model => withSkills(model, getSkills(model) with { Attack = before.Attack }));
            }

            if (before.Defense != after.Defense)
            {
                sink.Add(ChangeAreas.PrimarySkills, "Defense", before.Defense.ToString(), after.Defense.ToString(), model => withSkills(model, getSkills(model) with { Defense = before.Defense }));
            }

            if (before.SpellPower != after.SpellPower)
            {
                sink.Add(ChangeAreas.PrimarySkills, "Spell power", before.SpellPower.ToString(), after.SpellPower.ToString(), model => withSkills(model, getSkills(model) with { SpellPower = before.SpellPower }));
            }

            if (before.Knowledge != after.Knowledge)
            {
                sink.Add(ChangeAreas.PrimarySkills, "Knowledge", before.Knowledge.ToString(), after.Knowledge.ToString(), model => withSkills(model, getSkills(model) with { Knowledge = before.Knowledge }));
            }
        }

        /// <summary>
        /// Adds the change of the experience of a hero, if its experience or its level differs: "Experience", with the experience and the level
        /// before and after ("1,500 (level 3)"). The two are put back together, so the level and the experience stay in step.
        /// </summary>
        /// <typeparam name="TModel">The model of the hero.</typeparam>
        /// <param name="sink">Collects the change.</param>
        /// <param name="before">The level as loaded.</param>
        /// <param name="after">The level now.</param>
        /// <param name="withLevel">Returns the model with the given level and experience.</param>
        public static void AddHeroLevel<TModel>(this ChangeSink<TModel> sink, HeroLevel before, HeroLevel after, Func<TModel, HeroLevel, TModel> withLevel)
        {
            if (before == after)
            {
                return;
            }

            sink.Add(ChangeAreas.General, "Experience", LevelText(before), LevelText(after), model => withLevel(model, before));
        }

        /// <summary>
        /// Adds the change of where a hero stands, if its tile, its level or the way it faces differs: "Position", with the place and the facing
        /// before and after ("at 12, 5, Surface, facing North"; without the level in a game whose map has one). The whole position is put back.
        /// </summary>
        /// <typeparam name="TModel">The model of the hero.</typeparam>
        /// <param name="sink">Collects the change.</param>
        /// <param name="before">The position as loaded.</param>
        /// <param name="after">The position now.</param>
        /// <param name="hasLevels">Whether the map of the game has two levels, so a place says which level it is on.</param>
        /// <param name="withPosition">Returns the model with the given position.</param>
        public static void AddPosition<TModel>(this ChangeSink<TModel> sink, HeroPosition before, HeroPosition after, bool hasLevels, Func<TModel, HeroPosition, TModel> withPosition)
        {
            if (before == after)
            {
                return;
            }

            sink.Add(ChangeAreas.General, "Position", PositionText(before, hasLevels), PositionText(after, hasLevels), model => withPosition(model, before));
        }

        /// <summary>
        /// Adds the change of the tile a hero is going to, if it differs: "Destination", with the place before and after, or "none" when it goes nowhere.
        /// </summary>
        /// <typeparam name="TModel">The model of the hero.</typeparam>
        /// <param name="sink">Collects the change.</param>
        /// <param name="before">The destination as loaded, or <see cref="MapPosition.None"/>.</param>
        /// <param name="after">The destination now, or <see cref="MapPosition.None"/>.</param>
        /// <param name="hasLevels">Whether the map of the game has two levels, so a place says which level it is on.</param>
        /// <param name="withDestination">Returns the model with the given destination.</param>
        public static void AddDestination<TModel>(this ChangeSink<TModel> sink, MapPosition before, MapPosition after, bool hasLevels, Func<TModel, MapPosition, TModel> withDestination)
        {
            if (before == after)
            {
                return;
            }

            sink.Add(ChangeAreas.General, "Destination", PlaceText(before, hasLevels), PlaceText(after, hasLevels), model => withDestination(model, before));
        }

        /// <summary>
        /// Adds the change of the movement points of a hero, if they differ: "Movement points", with what is left and what a full day gives,
        /// before and after ("320 / 1,500"). Both are put back.
        /// </summary>
        /// <typeparam name="TModel">The model of the hero.</typeparam>
        /// <param name="sink">Collects the change.</param>
        /// <param name="before">The movement points as loaded.</param>
        /// <param name="after">The movement points now.</param>
        /// <param name="withMovement">Returns the model with the given movement points.</param>
        public static void AddMovement<TModel>(this ChangeSink<TModel> sink, MovementPoints before, MovementPoints after, Func<TModel, MovementPoints, TModel> withMovement)
        {
            if (before == after)
            {
                return;
            }

            sink.Add(ChangeAreas.General, "Movement points", MovementText(before), MovementText(after), model => withMovement(model, before));
        }

        /// <summary>
        /// Adds the change of the spell points of a hero, if they differ: "Spell points", with the points before and after.
        /// </summary>
        /// <typeparam name="TModel">The model of the hero.</typeparam>
        /// <param name="sink">Collects the change.</param>
        /// <param name="before">The spell points as loaded.</param>
        /// <param name="after">The spell points now.</param>
        /// <param name="withSpellPoints">Returns the model with the given spell points.</param>
        public static void AddSpellPoints<TModel>(this ChangeSink<TModel> sink, int before, int after, Func<TModel, int, TModel> withSpellPoints)
        {
            if (before == after)
            {
                return;
            }

            sink.Add(ChangeAreas.General, "Spell points", before.ToString("N0"), after.ToString("N0"), model => withSpellPoints(model, before));
        }

        /// <summary>
        /// Adds the change of the secondary skills of a hero, if the skills, their levels or their slots differ: "Skills", with the skills
        /// in the order of their slots and their levels before and after ("Archery (Beginner), Wisdom (Expert)"), followed by the skills
        /// that have a level but no slot ("not shown: Luck (Expert)"), or "none". The skills are slots in an order, so they are one change
        /// that puts the whole set back: a skill put back on its own would land in another slot.
        /// </summary>
        /// <typeparam name="TModel">The model of the hero.</typeparam>
        /// <typeparam name="TSkill">The secondary skill of the game.</typeparam>
        /// <typeparam name="TSkills">The secondary skills of a hero of the game.</typeparam>
        /// <param name="sink">Collects the change.</param>
        /// <param name="skills">Every secondary skill of the game, to find the skills that have a level but no slot.</param>
        /// <param name="before">The skills as loaded.</param>
        /// <param name="after">The skills now.</param>
        /// <param name="withSkills">Returns the model with the given skills.</param>
        public static void AddSecondarySkills<TModel, TSkill, TSkills>(
            this ChangeSink<TModel> sink,
            IReadOnlyCollection<TSkill> skills,
            TSkills before,
            TSkills after,
            Func<TModel, TSkills, TModel> withSkills)
            where TSkill : ISecondarySkill
            where TSkills : ISecondarySkills<TSkill, TSkills>
        {
            var (was, now) = (SkillsText(skills, before), SkillsText(skills, after));
            if (was == now)
            {
                return;
            }

            sink.Add(ChangeAreas.SecondarySkills, "Skills", was, now, model => withSkills(model, before));
        }

        /// <summary>
        /// Adds a change for each spell that was learned or forgotten: the name of the spell, with "learned" or "not learned" before and
        /// after, in the order of the codes of the spells. Each is put back on its own.
        /// </summary>
        /// <typeparam name="TModel">The model of the hero.</typeparam>
        /// <typeparam name="TSpell">The spell of the game.</typeparam>
        /// <param name="sink">Collects the changes.</param>
        /// <param name="before">The learned spells as loaded.</param>
        /// <param name="after">The learned spells now.</param>
        /// <param name="withLearned">Returns the model with the given spell learned (<c>true</c>) or not learned (<c>false</c>).</param>
        public static void AddLearnedSpells<TModel, TSpell>(this ChangeSink<TModel> sink, SpellSet<TSpell> before, SpellSet<TSpell> after, Func<TModel, TSpell, bool, TModel> withLearned)
            where TSpell : class, IGameEntity
        {
            foreach (var spell in before.Concat(after).DistinctBy(s => s.Code).OrderBy(s => s.Code))
            {
                var (had, has) = (before.Contains(spell), after.Contains(spell));
                if (had != has)
                {
                    sink.Add(ChangeAreas.Spells, spell.Name, LearnedText(had), LearnedText(has), model => withLearned(model, spell, had));
                }
            }
        }

        /// <summary>
        /// Adds a change for each resource whose amount differs, each put back on its own.
        /// </summary>
        /// <typeparam name="TModel">The model of the record that holds the resources.</typeparam>
        /// <param name="sink">Collects the changes.</param>
        /// <param name="before">The resources as loaded.</param>
        /// <param name="after">The resources now.</param>
        /// <param name="getResources">Gets the resources of a model.</param>
        /// <param name="withResources">Returns the model with the given resources.</param>
        public static void AddResources<TModel>(
            this ChangeSink<TModel> sink,
            ResourceSet before,
            ResourceSet after,
            Func<TModel, ResourceSet> getResources,
            Func<TModel, ResourceSet, TModel> withResources)
        {
            foreach (var resource in ResourceSet.All)
            {
                var (was, now, which) = (before[resource], after[resource], resource);
                if (was != now)
                {
                    sink.Add(ChangeAreas.Resources, which.ToString(), was.ToString("N0"), now.ToString("N0"), model => withResources(model, getResources(model).With(which, was)));
                }
            }
        }

        /// <summary>
        /// Adds a change for each place of the tavern whose hero differs, with the name of the hero.
        /// </summary>
        /// <typeparam name="TModel">The model of the record that holds the tavern.</typeparam>
        /// <param name="sink">Collects the changes.</param>
        /// <param name="before">The tavern as loaded.</param>
        /// <param name="after">The tavern now.</param>
        /// <param name="heroName">Gives the name of the hero with an id.</param>
        /// <param name="getTavern">Gets the tavern of a model.</param>
        /// <param name="withTavern">Returns the model with the given tavern.</param>
        public static void AddTavern<TModel>(
            this ChangeSink<TModel> sink,
            TavernHeroes before,
            TavernHeroes after,
            Func<int, string> heroName,
            Func<TModel, TavernHeroes> getTavern,
            Func<TModel, TavernHeroes, TModel> withTavern)
        {
            for (var place = 0; place < TavernHeroes.Places; place++)
            {
                var (was, now, index) = (before.Ids[place], after.Ids[place], place);
                if (was != now)
                {
                    sink.Add(
                        ChangeAreas.General,
                        $"Tavern hero {place + 1}",
                        was is { } b ? heroName(b) : NoHero,
                        now is { } a ? heroName(a) : NoHero,
                        model => withTavern(model, getTavern(model).With(index, was)));
                }
            }
        }

        /// <summary>
        /// Adds the change of the days a player has left to own a town, if they differ.
        /// </summary>
        /// <typeparam name="TModel">The model of the player.</typeparam>
        /// <param name="sink">Collects the change.</param>
        /// <param name="before">The days as loaded, or <c>null</c> when no countdown ran.</param>
        /// <param name="after">The days now.</param>
        /// <param name="withDaysLeft">Returns the model with the given days.</param>
        public static void AddDaysLeft<TModel>(this ChangeSink<TModel> sink, int? before, int? after, Func<TModel, int?, TModel> withDaysLeft)
        {
            if (before == after)
            {
                return;
            }

            sink.Add(ChangeAreas.General, "Days left without a castle", DaysText(before), DaysText(after), model => withDaysLeft(model, before));
        }

        private static string DaysText(int? days) => days?.ToString() ?? "not counting";

        private static string LevelText(HeroLevel level) => $"{level.Experience:N0} (level {level.Level})";

        private static string LearnedText(bool learned) => learned ? "learned" : "not learned";

        private static string MovementText(MovementPoints points) => $"{points.Current:N0} / {points.Max:N0}";

        private static string SkillsText<TSkill, TSkills>(IReadOnlyCollection<TSkill> all, TSkills skills)
            where TSkill : ISecondarySkill
            where TSkills : ISecondarySkills<TSkill, TSkills>
        {
            var parts = Enumerable.Range(0, skills.SlotCount)
                .Select(skills.GetSkillInSlot)
                .Where(skill => skill is not null)
                .Select(skill => SkillText(skill, skills.GetSkillLevel(skill)))
                .ToList();

            // Skills without a slot are applied by the game, but not shown on the hero screen.
            var hidden = all
                .Where(skill => skills.GetSlotOf(skill) < 0 && skills.GetSkillLevel(skill) != SkillLevel.None)
                .OrderBy(skill => skill.Code)
                .Select(skill => SkillText(skill, skills.GetSkillLevel(skill)))
                .ToList();
            if (hidden.Count > 0)
            {
                parts.Add($"not shown: {string.Join(", ", hidden)}");
            }

            return parts.Count == 0 ? ChangeText.None : string.Join(", ", parts);
        }

        private static string SkillText(ISecondarySkill skill, SkillLevel level) => $"{skill.Name} ({(level == SkillLevel.None ? "no level" : level.Name)})";

        private static string PlaceText(MapPosition place, bool hasLevels) =>
            place == MapPosition.None ? ChangeText.None : MapLevelText.Coordinates(place, hasLevels);

        private static string PositionText(HeroPosition position, bool hasLevels) =>
            $"{PlaceText(new MapPosition(position.X, position.Y, position.Level), hasLevels)}, facing {position.FacingDirection}";
    }
}
