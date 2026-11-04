namespace Siniskalc.Homm3.ViewModels
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>User-facing descriptions shared by every supported creature-bank kind.</summary>
    public sealed class CreatureBankTextProvider
    {
        public IReadOnlyList<string> DefeatChoices => ["Undefeated", "Defeated"];

        public string GroupName(CreatureBankKind kind) => kind switch
        {
            CreatureBankKind.DragonUtopia => "Dragon Utopias",
            CreatureBankKind.CyclopsStockpile => "Cyclops Stockpiles",
            CreatureBankKind.DwarvenTreasury => "Dwarven Treasuries",
            CreatureBankKind.ImpCache => "Imp Caches",
            CreatureBankKind.MedusaStores => "Medusa Stores",
            _ => "Naga Banks",
        };

        public string Name(CreatureBankKind kind, int number) => $"{kind switch
        {
            CreatureBankKind.DragonUtopia => "Utopia",
            CreatureBankKind.CyclopsStockpile => "Stockpile",
            CreatureBankKind.DwarvenTreasury => "Treasury",
            CreatureBankKind.ImpCache => "Imp Cache",
            CreatureBankKind.MedusaStores => "Medusa Stores",
            _ => "Naga Bank",
        }} {number}";

        public string Preset(CreatureBankKind kind, int strength, EntityCatalog catalog) =>
            $"{string.Join(", ", this.Garrison(kind, strength, false, catalog))}; {this.Resources(kind, strength)}";

        public string Heading(CreatureBankModel model, int number) => $"{this.Name(model.Kind, number)} at {this.Location(model.Kind, model.Position)}";

        public string Status(bool cleared) => cleared ? "Defeated" : "Uncleared";

        public string Location(CreatureBankKind kind, MapPosition position) => $"{MapLevelText.Place(position.X - (kind == CreatureBankKind.ImpCache ? 0 : 1), position.Y)}, {(position.Level == MapPosition.Above ? "Surface" : "Underground")}";

        public string RewardSummary(CreatureBankModel model) =>
            string.Join(", ", new[] { this.Resources(model.Kind, model.Strength, compact: true) }.Concat(model.Artifacts.Select(a => a?.Name ?? "(empty)")));

        public IReadOnlyList<string> Garrison(CreatureBankKind kind, int strength, bool upgradedCenter, EntityCatalog catalog)
        {
            if (kind == CreatureBankKind.DragonUtopia)
            {
                var dragons = DragonUtopiaGuards.Counts(strength);
                return [$"{dragons[0]} Green Dragons", $"{dragons[1]} Red Dragons", $"{dragons[2]} Gold Dragons", $"{dragons[3]} Black {(dragons[3] == 1 ? "Dragon" : "Dragons")}"];
            }

            var types = CreatureBankPresets.Types(kind, upgradedCenter);
            var counts = CreatureBankPresets.Counts(kind, strength);
            return types.Select((code, slot) => (Code: code, Count: counts[slot])).Where(s => s.Code >= 0)
                .GroupBy(s => s).Select(g => $"{g.Count()} x {g.Key.Count} {catalog.Creatures.Find((byte)g.Key.Code)!.Name}").ToList();
        }

        public string Resources(CreatureBankKind kind, int strength, bool compact = false) =>
            string.Join(", ", CreatureBankPresets.Resources(kind, strength).Select((count, index) => (Count: count, Resource: (Resource)index)).Where(r => r.Count > 0)
                .Select(r => compact && r.Resource == Resource.Gold
                    ? $"{r.Count / 1000m:0.###}k g"
                    : $"{r.Count.ToString("N0", CultureInfo.CurrentCulture)} {r.Resource.ToString().ToLowerInvariant()}"));

        public string Summary(string group, int total, int defeated) => $"{total} {group}: {total - defeated} uncleared, {defeated} defeated";

        public string ArtifactSlot(int slot) => $"Artifact {slot + 1}";

        public string MapSummary(int merchants, int banks) => string.Join(
            ", ",
            new[]
            {
                merchants > 0 ? $"{merchants} {(merchants == 1 ? "merchant" : "merchants")}" : null,
                banks > 0 ? $"{banks} creature {(banks == 1 ? "bank" : "banks")}" : null,
            }.Where(part => part != null));

        /// <summary>
        /// The short count shown next to the Map node of the navigation tree; a part the map does not have is left out.
        /// </summary>
        public string MapNavSummary(int merchants, int banks) => string.Join(
            ", ",
            new[]
            {
                merchants > 0 ? $"{merchants} {(merchants == 1 ? "merchant" : "merchants")}" : null,
                banks > 0 ? $"{banks} {(banks == 1 ? "bank" : "banks")}" : null,
            }.Where(part => part != null));
    }
}
