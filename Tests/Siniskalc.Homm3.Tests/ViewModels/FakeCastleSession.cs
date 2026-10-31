using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Heroes.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.ViewModels
{
    /// <summary>
    /// A castle session over no savefile: it holds the castle it was given, applies the edits it is sent, tells its history events as the
    /// real one does, and answers the facts of the savefile (the heroes, the game version) with what a test sets.
    /// </summary>
    internal sealed class FakeCastleSession : ICastleSession, IHeroSummaries<PlayerColor>
    {
        private readonly FakeHistory history = new();

        public FakeCastleSession(CastleModel castle)
        {
            this.Current = castle;
            this.Original = castle;
        }

        public CastleModel Current { get; private set; }

        public CastleModel Original { get; }

        public bool IsDirty => !ReferenceEquals(this.Current, this.Original);

        public int Index { get; init; } = 2;

        public string Name => this.Current.Name;

        public GameVersion GameVersion { get; init; } = GameVersion.HornOfTheAbyss;

        /// <summary>Gets the heroes of the savefile by id.</summary>
        public Dictionary<int, HeroModel> HeroModels { get; } = [];

        public IHeroSummaries<PlayerColor> Heroes => this;

        /// <summary>Gets or sets what happens when the page gives the castle to a player; it may throw the rule that forbids it.</summary>
        public Action<PlayerColor> OnChangeOwner { get; set; } = _ => { };

        public IByteEditTarget Target { get; } = new Mock<IByteEditTarget>().Object;

        public IByteEditTargetFactory? FactoryAsked { get; private set; }

        public IHistoryEvents Events => this.history;

        public SaveDocumentHistory History => throw new NotSupportedException("The fake has no savefile.");

        public HeroModel? FindHero(int heroId) => this.HeroModels.GetValueOrDefault(heroId);

        public void ChangeOwner(PlayerColor owner)
        {
            this.OnChangeOwner(owner);
            this.Update(c => c with { Owner = owner });
        }

        public IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory)
        {
            this.FactoryAsked = factory;
            return this.Target;
        }

        public void Update(Func<CastleModel, CastleModel> edit)
        {
            this.Current = edit(this.Current);
            this.history.Raise();
        }

        /// <summary>Tells the page that the history changed with no change to this castle, as an edit of another record does.</summary>
        public void RaiseHistoryChangedElsewhere() => this.history.Raise();

        IReadOnlyList<HeroSummary<PlayerColor>> IHeroSummaries<PlayerColor>.GetSummaries() =>
            [.. this.HeroModels.Select((h, i) => Summary(i, h.Key, h.Value))];

        int IHeroSummaries<PlayerColor>.IndexOfId(int heroId) => this.HeroModels.Keys.ToList().IndexOf(heroId);

        HeroSummary<PlayerColor>? IHeroSummaries<PlayerColor>.FindById(int heroId) =>
            this.HeroModels.Keys.ToList().IndexOf(heroId) is var index and >= 0 ? Summary(index, heroId, this.HeroModels[heroId]) : null;

        private static HeroSummary<PlayerColor> Summary(int index, int heroId, HeroModel hero) =>
            new(index, heroId, hero.Name, hero.Owner, hero.HeroClass.Name, hero.HeroLevel.Level);

        private sealed class FakeHistory : IHistoryEvents
        {
            public event EventHandler? Changed;

            public void Raise() => this.Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
