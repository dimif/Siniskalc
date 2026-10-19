namespace Siniskalc.Homm3.Tests.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Homm3.Tests.Data;

    /// <summary>
    /// Tests what a castle of HoMM3 tells through the interface every game's castle has.
    /// </summary>
    [TestFixture]
    internal sealed class CastleModelTests
    {
        [Test]
        public void ACastleTellsWhatEveryGameCastleHasThroughTheSharedInterface()
        {
            var castles = CastleSamples.Load(CastleSamples.Folder("Bulwark").First()).History.Current.Castles();
            var model = TestServices.CastleDeserializer.Deserialize(castles.First(c => c.Name == "Still Water"));

            ICastleModel<PlayerColor, Creature, Faction> castle = model;

            Assert.That((castle.Name, castle.Faction?.Name), Is.EqualTo(("Still Water", model.Faction!.Name)));
            Assert.That(castle.Faction, Is.SameAs(model.Faction));
            Assert.That((castle.CreaturesAvailable.Count, castle.Garrison.Slots.Count), Is.EqualTo((7, 7)));
            Assert.That(castle.Garrison, Is.SameAs(model.Garrison));
        }
    }
}
