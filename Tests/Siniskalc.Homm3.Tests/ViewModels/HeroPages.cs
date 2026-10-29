using Moq;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.ViewModels
{
    /// <summary>The hero page over a fake hero session, built the way the application builds it.</summary>
    internal static class HeroPages
    {
        public static HeroViewModel Create(FakeHeroSession session) =>
            new(session, new EffectLineProvider(), new SpellPointsCalculatorFactory().Create(session.GameVersion), new Mock<IByteEditTargetFactory>().Object, openCastle: _ => { });
    }
}
