using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.ViewModels
{
    /// <summary>
    /// The send to castle command of the position card on the roe_61_inferno_garrison_and_visiting savegame, where Tazar visits "4_Inferno".
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class HeroSendToCastleTests
    {
        [Test]
        public void ACastleOfThePlayerIsListedAtItsGateAndIsTakenWhileAnotherHeroVisitsIt()
        {
            var path = Path.Combine("TestData", "Roe", "roe_61_inferno_garrison_and_visiting.GM1");
            var main = TestServices.CreateHomm3Editor();
            main.LoadGameFromBytes(TestServices.SavefileCodec.Read(path), path);
            var castles = main.LoadedGame().History.Current.Castles();
            var inferno = castles.GetModel(castles.FindByName("4_Inferno"));
            var owner = inferno.Owner;
            var heroes = main.LoadedGame().History.Current.Heroes();
            var other = heroes.GetModels().First(h => h.Owner == owner && h.Id != inferno.VisitingHeroId && !string.IsNullOrEmpty(h.Name));

            Assert.That(main.ShowHero(other.Name), Is.True);
            var send = main.CurrentPageOf<HeroViewModel>().GeneralInfoViewModel.Position.SendToCastle!;

            var listed = send.Castles.Single(c => c.Name == "4_Inferno");
            Assert.That((listed.Entrance.X, listed.Entrance.Y, listed.Entrance.Level), Is.EqualTo((inferno.Position.X, inferno.Position.Y, inferno.Position.Level)));
            Assert.That(listed.IsOccupied, Is.True, "Tazar visits it");
            Assert.That(send.Castles.All(c => castles.GetModels().Any(m => m.Name == c.Name && m.Owner == owner)), Is.True, "only the castles of the player");
        }
    }
}
