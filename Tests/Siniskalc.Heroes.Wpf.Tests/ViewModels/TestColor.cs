namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// A made-up colour of a made-up game, so the tests need no real game.
    /// </summary>
    internal sealed class TestColor : AbstractEnumeration, IPlayerColor
    {
        public static readonly TestColor Red = new("Red", 0);

        public static readonly TestColor Blue = new("Blue", 1);

        private TestColor(string name, byte code)
            : base(name, code)
        {
        }
    }
}
