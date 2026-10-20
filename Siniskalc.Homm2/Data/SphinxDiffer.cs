namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what changed in a sphinx: whether it is active, the reward, the answers and the riddle.
    /// </summary>
    public sealed class SphinxDiffer : IMapObjectDiffer<SphinxModel>
    {
        private readonly IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges;

        /// <param name="pendingChanges">Makes the pending changes of what the differ finds.</param>
        public SphinxDiffer(IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges)
        {
            this.pendingChanges = pendingChanges;
        }

        private static readonly string[] Resources = ["Wood", "Mercury", "Ore", "Sulfur", "Crystal", "Gems", "Gold"];

        public IReadOnlyList<PendingChange> Diff(int index, SphinxModel original, SphinxModel current)
        {
            var changes = new List<PendingChange>();

            void Add(string what, string before, string after, System.Func<SphinxModel, SphinxModel> revert) =>
                changes.Add(this.pendingChanges.ForMapObject<SphinxModel>(Homm2ChunkKinds.Sphinxes, current.Name, index, new Change(ChangeAreas.General, what, before, after), revert));

            if (original.IsActive != current.IsActive)
            {
                Add("Riddle", original.IsActive ? "not answered" : "answered", current.IsActive ? "not answered" : "answered", s => s with { IsActive = original.IsActive });
            }

            for (var i = 0; i < Resources.Length; i++)
            {
                var resource = i;
                if (original.Reward[i] != current.Reward[i])
                {
                    Add($"Reward: {Resources[i]}", original.Reward[i].ToString("N0"), current.Reward[i].ToString("N0"), s => s with { Reward = s.Reward.Select((v, j) => j == resource ? original.Reward[resource] : v).ToList() });
                }
            }

            if (original.ArtifactCode != current.ArtifactCode)
            {
                Add("Reward: artifact", original.ArtifactCode.ToString(), current.ArtifactCode.ToString(), s => s with { ArtifactCode = original.ArtifactCode });
            }

            if (!original.Answers.SequenceEqual(current.Answers))
            {
                Add("Answers", string.Join(", ", original.Answers), string.Join(", ", current.Answers), s => s with { Answers = original.Answers });
            }

            if (original.Riddle != current.Riddle)
            {
                Add("Riddle text", original.Riddle, current.Riddle, s => s with { Riddle = original.Riddle });
            }

            return changes;
        }
    }
}
