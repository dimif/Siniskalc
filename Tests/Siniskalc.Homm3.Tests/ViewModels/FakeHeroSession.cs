using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.ViewModels
{
    /// <summary>
    /// A hero session over no savefile: it holds the hero it was given, applies the edits it is sent, tells its history events as the real
    /// one does (also with a new revision), and answers the facts of the savefile with what a test sets.
    /// </summary>
    internal sealed class FakeHeroSession : IHeroSession
    {
        private readonly FakeHistory history = new();

        public FakeHeroSession(HeroModel hero)
        {
            this.Current = hero;
            this.Original = hero;
        }

        public HeroModel Current { get; private set; }

        public HeroModel Original { get; }

        public bool IsDirty => !ReferenceEquals(this.Current, this.Original);

        public object Revision { get; private set; } = new();

        public int Index => this.HeroIndex;

        public int HeroIndex { get; init; } = 3;

        public GameVersion GameVersion { get; init; } = GameVersion.HornOfTheAbyss;

        public bool SupportsAsleep { get; init; } = true;

        public int GarrisonCastleIndex { get; set; } = -1;

        public Dictionary<int, string> CastleNames { get; } = [];

        /// <summary>Gets or sets what happens to an edit before it is applied, with the edited hero; it may throw the rule that forbids it.</summary>
        public Action<HeroModel> OnUpdate { get; set; } = _ => { };

        public IByteEditTarget Target { get; } = new Mock<IByteEditTarget>().Object;

        public IByteEditTargetFactory? FactoryAsked { get; private set; }

        public IHistoryEvents Events => this.history;

        public SaveDocumentHistory History => throw new NotSupportedException("The fake has no savefile.");

        public string CastleName(int castleIndex) => this.CastleNames[castleIndex];

        public IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory)
        {
            this.FactoryAsked = factory;
            return this.Target;
        }

        public void Update(Func<HeroModel, HeroModel> edit)
        {
            var edited = edit(this.Current);
            this.OnUpdate(edited);
            this.Current = edited;
            this.Revision = new object();
            this.history.Raise();
        }

        /// <summary>Tells the page that the history changed with no change to this hero, as an edit of another hero or a save does.</summary>
        public void RaiseHistoryChangedElsewhere() => this.history.Raise();

        private sealed class FakeHistory : IHistoryEvents
        {
            public event EventHandler? Changed;

            public void Raise() => this.Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
