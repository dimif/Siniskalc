namespace Siniskalc.Heroes.Data
{
    using System;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// The changes that every game's differs report in the same way: who owns a record, what is in the slots of an army, the skills of a hero, and the resources, the tavern and the countdown of a player.
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
    }
}
