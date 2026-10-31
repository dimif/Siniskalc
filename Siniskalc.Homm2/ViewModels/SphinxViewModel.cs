namespace Siniskalc.Homm2.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A sphinx of the map: whether its riddle can still be answered, what an answer gives, the answers it accepts and the riddle. The block of
    /// the sphinx has a fixed size, so the answers have 12 characters each and the riddle the room the block has.
    /// </summary>
    public sealed class SphinxViewModel : AbstractViewModelBase
    {
        private static readonly Resource[] ResourceOrder = [Resource.Gold, Resource.Wood, Resource.Ore, Resource.Mercury, Resource.Sulfur, Resource.Crystal, Resource.Gems];

        private readonly IEditSession<SphinxModel> session;

        public SphinxViewModel(IEditSession<SphinxModel> session, Siniskalc.Homm2.Entities.GameVersion version)
        {
            this.session = session;
            this.SolvedToggle = new ToggleRowViewModel(
                "Solved",
                () => !this.session.Current.IsActive,
                solved => this.session.Update(sphinx => sphinx with { IsActive = !solved }),
                toolTip: "The game ticks this when a hero answers the riddle (and gives the reward); unticking it lets any hero answer again. Not tried in the game.");
            this.Reward = ResourceOrder.Select(this.CreateRewardRow).ToList();
            this.ArtifactPicker = new ArtifactPickerViewModel(
                "Artifact",
                onPick => new ArtifactFinderViewModel(version, string.Empty, "Choose", artifact => onPick(artifact.Name)),
                name => this.session.Update(sphinx => sphinx with { ArtifactCode = name == null ? SphinxModel.NoArtifact : Artifact.All.First(a => a.Name == name).Code }));
            this.Answers = Enumerable.Range(0, Constants.Sphinx.MaxAnswers).Select(this.CreateAnswerField).ToList();
            this.Riddle = new TextFieldViewModel(
                "Riddle",
                () => this.session.Current.Riddle,
                text => this.session.Update(sphinx => sphinx with { Riddle = text }),
                () => this.session.Current.RiddleCapacity,
                isMultiline: true,
                toolTip: "What the sphinx asks. It has the room of the original riddle: a longer one does not fit the savegame.");
        }

        public int Index => this.session.Index;

        public string Title => this.session.Current.Name;

        public ToggleRowViewModel SolvedToggle { get; }

        public IReadOnlyList<AmountRowViewModel> Reward { get; }

        /// <summary>
        /// Gets the artifact an answer gives, picked like the artifact of a hero; empty when it gives none.
        /// </summary>
        public ArtifactPickerViewModel ArtifactPicker { get; }

        /// <summary>
        /// Gets the eight places for the answers: the answers in order, then empty places. Emptying a place takes the answer away; at least one stays.
        /// </summary>
        public IReadOnlyList<TextFieldViewModel> Answers { get; }

        public TextFieldViewModel Riddle { get; }

        public void Refresh()
        {
            this.SolvedToggle.Refresh();
            this.ArtifactPicker.UpdateFromModel(this.session.Current.ArtifactCode == SphinxModel.NoArtifact ? null : Artifact.FromByte((byte)this.session.Current.ArtifactCode)?.Name);
            foreach (var row in this.Reward)
            {
                row.Refresh();
            }

            foreach (var answer in this.Answers)
            {
                answer.Refresh();
            }

            this.Riddle.Refresh();
            this.RaiseAllPropertiesChanged();
        }

        private static IReadOnlyList<string> WithAnswer(IReadOnlyList<string> answers, int place, string text)
        {
            var places = Enumerable.Range(0, Constants.Sphinx.MaxAnswers).Select(i => i < answers.Count ? answers[i] : string.Empty).ToList();
            places[place] = text.Trim();
            var result = places.Where(a => a.Length > 0).ToList();
            return result.Count > 0 ? result : answers;
        }

        private AmountRowViewModel CreateRewardRow(Resource resource)
        {
            var gold = resource == Resource.Gold;
            return new AmountRowViewModel(
                resource.ToString(),
                ResourcePalette.Of(resource.ToString()),
                0,
                gold ? Constants.Player.MaxGold : Constants.Player.MaxOther,
                gold ? Constants.Player.GoldStep : Constants.Player.OtherStep,
                () => this.session.Current.Reward[(int)resource],
                () => this.session.Original.Reward[(int)resource],
                amount => this.session.Update(sphinx => sphinx with { Reward = sphinx.Reward.Select((value, i) => i == (int)resource ? (int)amount : value).ToList() }));
        }

        private TextFieldViewModel CreateAnswerField(int place) => new(
            $"Answer {place + 1}",
            () => place < this.session.Current.Answers.Count ? this.session.Current.Answers[place] : string.Empty,
            text => this.session.Update(sphinx => sphinx with { Answers = WithAnswer(sphinx.Answers, place, text) }),
            () => Constants.Sphinx.MaxAnswerLength,
            toolTip: "An answer the sphinx accepts, in any case; at most 12 characters.");
    }
}
