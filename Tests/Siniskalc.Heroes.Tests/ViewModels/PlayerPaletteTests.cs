namespace Siniskalc.Heroes.Tests.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.ViewModels;

    /// <summary>
    /// Tests the colours the editor gives the players.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerPaletteTests
    {
        private sealed record Color(string Name, byte Code) : IPlayerColor
        {
            public bool IsNeutral => false;
        }

        [Test]
        public void EachColourHasItsOwnHexValue()
        {
            var hexes = PlayerPalette.Names.Select(n => PlayerPalette.Of(new Color(n, 0))).ToList();

            Assert.That(hexes, Is.Unique);
            Assert.That(hexes, Does.Not.Contain(PlayerPalette.NoPlayer));
        }

        [Test]
        public void WhatNobodyOwnsAndAColourTheEditorDoesNotKnowGetTheNeutralColour()
        {
            Assert.That(PlayerPalette.Of(null), Is.EqualTo(PlayerPalette.NoPlayer));
            Assert.That(PlayerPalette.Of(new Color("Chartreuse", 9)), Is.EqualTo(PlayerPalette.NoPlayer));
        }

        [Test]
        public void TheNameOfAColourDecidesItsHexValueWhateverItsCode()
        {
            Assert.That(PlayerPalette.Of(new Color("red", 0)), Is.EqualTo(PlayerPalette.Of(new Color("Red", 2))));
        }
    }
}
