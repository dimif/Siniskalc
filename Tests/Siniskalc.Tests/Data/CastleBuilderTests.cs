using static Siniskalc.Tests.Data.CastleMasks;
using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// Checks <see cref="CastleBuilder"/> against the savefiles in which a game built one building after the other:
    /// building what the next file shows on top of the previous file has to give the masks of the next file, and removing it
    /// from the next file the masks of the previous one.
    /// </summary>
    [TestFixture]
    internal sealed partial class CastleBuilderTests
    {
        private static readonly string[] Folders = ["Rampart", "Bulwark"];

        private static IEnumerable<(string Name, CastleChunk Before, CastleChunk After, string Save)> Steps(string folder)
        {
            var saves = CastleSamples.Folder(folder).Where(s => !Path.GetFileName(s).Contains("milestone")).ToList();
            for (var i = 1; i < saves.Count; i++)
            {
                var before = CastleSamples.Load(saves[i - 1]).History.Current.Castles();
                var after = CastleSamples.Load(saves[i]).History.Current.Castles();
                foreach (var castle in after.Where(c => before.FindByName(c.Name) >= 0 && c.GetByteAt(CastleLayout.Owner) != Constants.ByteOff))
                {
                    yield return (castle.Name, before[before.FindByName(castle.Name)], castle, Path.GetFileName(saves[i]));
                }
            }
        }

        private static IEnumerable<Building> Added(CastleModel before, CastleModel after) =>
            after.Buildings.Except(before.Buildings).Where(b => !CastleBuilder.IsAutomatic(b));

        [Test]
        public void BuildingWhatTheNextSaveShowsGivesTheMasksOfTheNextSave()
        {
            var checkedSteps = 0;
            var failures = new List<string>();
            foreach (var folder in Folders)
            {
                foreach (var (name, before, after, save) in Steps(folder))
                {
                    var model = TestServices.CastleDeserializer.Deserialize(before);
                    var added = Added(model, TestServices.CastleDeserializer.Deserialize(after)).ToList();
                    if (added.Count != 1)
                    {
                        continue;
                    }

                    var built = TestServices.SerializeCastle(before, CastleBuilder.Build(model, added[0]));
                    if (Masks(built) != Masks(after))
                    {
                        failures.Add($"{save} {name} + {added[0].Name}: got {Hex(Masks(built))}, expected {Hex(Masks(after))}");
                    }

                    checkedSteps++;
                }
            }

            Assert.That(failures, Is.Empty);
            Assert.That(checkedSteps, Is.GreaterThan(60));
        }

        [Test]
        public void RemovingWhatTheNextSaveShowsGivesTheMasksOfThePreviousSave()
        {
            var checkedSteps = 0;
            var failures = new List<string>();
            foreach (var folder in Folders)
            {
                foreach (var (name, before, after, save) in Steps(folder))
                {
                    var model = TestServices.CastleDeserializer.Deserialize(after);
                    var added = Added(TestServices.CastleDeserializer.Deserialize(before), model).ToList();
                    if (added.Count != 1)
                    {
                        continue;
                    }

                    var removed = TestServices.SerializeCastle(after, CastleBuilder.Unbuild(model, added[0]));
                    if (Masks(removed) != Masks(before))
                    {
                        failures.Add($"{save} {name} - {added[0].Name}: got {Hex(Masks(removed))}, expected {Hex(Masks(before))}");
                    }

                    checkedSteps++;
                }
            }

            Assert.That(failures, Is.Empty);
            Assert.That(checkedSteps, Is.GreaterThan(60));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void BuildingEveryBuildingOfASavedCastleInAnyOrderGivesTheMasksOfThatSave(bool descending)
        {
            var replayed = 0;
            var failures = new List<string>();
            foreach (var path in CastleSamples.All().Where(p => VerifiedFactions.Any(f => Path.GetDirectoryName(p)!.EndsWith(f.Name))
                // In the savefile built with a cheat the upgraded frigate is missing from the current buildings of an inland Cove,
                // which no rule seen elsewhere explains, so it is not used as proof.
                && !p.EndsWith("cove_inland_all_cheat.GM1")))
            {
                foreach (var chunk in CastleSamples.Load(path).History.Current.Castles())
                {
                    var saved = TestServices.CastleDeserializer.Deserialize(chunk);
                    if (saved.Faction == null || !VerifiedFactions.Contains(saved.Faction))
                    {
                        continue;
                    }

                    // Start from a town that has nothing but the village hall, with the hidden bits of the companions cleared.
                    var model = saved with { Buildings = new HashSet<Building> { Building.VillageHall }, CurrentBuildings = new HashSet<Building> { Building.VillageHall } };
                    var empty = TestServices.SerializeCastle(chunk, model);
                    var order = saved.Buildings.Where(b => b != Building.VillageHall && !CastleBuilder.IsAutomatic(b));
                    foreach (var building in descending ? order.OrderByDescending(b => b.Code) : order.OrderBy(b => b.Code))
                    {
                        model = CastleBuilder.SetBuilt(model, building, true);
                    }

                    var rebuilt = TestServices.SerializeCastle(empty, model);
                    if (Masks(rebuilt) != Masks(chunk))
                    {
                        failures.Add($"{Path.GetFileName(path)} {chunk.Name}: got {Hex(Masks(rebuilt))}, expected {Hex(Masks(chunk))}");
                    }

                    replayed++;
                }
            }

            Assert.That(failures, Is.Empty);
            Assert.That(replayed, Is.GreaterThan(200));
        }
    }
}
