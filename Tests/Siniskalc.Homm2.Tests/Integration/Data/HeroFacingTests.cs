namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The direction a hero faces through the reader, the page and the writer, in the saves of MarkerFacing (TestData/Homm2/MarkerFacing),
    /// and in every save.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroFacingTests
    {
        private const int ColarBlau = 50;

        private static HeroModel Hero(int save) => TestServices.LoadMarkerFacing(save).History.Current.Heroes().GetModel(ColarBlau);

        [TestCase(91, FacingDirection.West)]
        [TestCase(92, FacingDirection.NorthWest)]
        [TestCase(93, FacingDirection.NorthEast)]
        public void WithoutAMarkerTheDestinationIsNoneAndTheFacingIsTheOneTheHeroTurnedTo(int save, FacingDirection facing)
        {
            var hero = Hero(save);

            Assert.That(hero.Destination, Is.EqualTo(MapPosition.None));
            Assert.That(hero.Position.FacingDirection, Is.EqualTo(facing));
        }

        [Test]
        public void EveryHeroOfEverySaveFacesOneOfEightDirections()
        {
            foreach (var path in Saves.All())
            {
                foreach (var chunk in TestServices.Load(path).History.Current.Heroes())
                {
                    Assert.That(chunk.GetByteAt(HeroLayout.Facing), Is.LessThanOrEqualTo(7), $"{Path.GetFileName(path)}: {chunk.Name}");
                }
            }
        }

        [Test]
        public void ThePositionCardEditsTheFacingAndWritesOnlyTheByteOfTheFacing()
        {
            var game = TestServices.LoadMarkerFacing(91);
            var page = new HeroViewModel(game, ColarBlau, _ => { }, TestServices.SpellPoints, TestServices.ByteEditTargets);
            Assert.That(page.Position.Facing, Is.EqualTo(FacingDirection.West));

            page.Position.Facing = FacingDirection.South;

            var bytes = game.GetBytesToSave();
            var changed = Enumerable.Range(0, bytes.Length).Where(i => bytes[i] != game.Bytes[i]).ToList();
            Assert.That(changed, Is.EqualTo(new[] { game.Layout.HeroStart(ColarBlau) + HeroLayout.Facing }));
            Assert.That(TestServices.Parser.Parse(bytes, null).History.Current.Heroes().GetModel(ColarBlau).Position.FacingDirection, Is.EqualTo(FacingDirection.South));
        }
    }
}
