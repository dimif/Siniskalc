namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// A mine of a made-up game, so the tests of the mines need no real game.
    /// </summary>
    internal sealed record TestMine(int Index, MineKind Kind, TestColor Owner, MapPosition Position) : IMineModel<TestColor>
    {
        public string Name => MineKinds.NameAt(this.Kind, this.Position, hasLevels: true);
    }
}
