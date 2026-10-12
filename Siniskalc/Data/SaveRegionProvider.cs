namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;

    /// <summary>
    /// Describes where each known field lives in the whole savefile: the header, the map, the setup of the game, the date, and
    /// every hero, player and castle, using the providers of the chunks and the locators of the rest. Bytes that no region
    /// covers are not known.
    /// </summary>
    public sealed class SaveRegionProvider : ISaveRegionProvider
    {
        private readonly IMapHeaderLocator mapHeaderLocator;
        private readonly IPlayerControlLocator setupLocator;
        private readonly IGameDateLocator dateLocator;
        private readonly IHeroRegionProvider heroRegions;
        private readonly IPlayerRegionProvider playerRegions;
        private readonly ICastleRegionProvider castleRegions;

        public SaveRegionProvider(
            IMapHeaderLocator mapHeaderLocator,
            IPlayerControlLocator setupLocator,
            IGameDateLocator dateLocator,
            IHeroRegionProvider heroRegions,
            IPlayerRegionProvider playerRegions,
            ICastleRegionProvider castleRegions)
        {
            this.mapHeaderLocator = mapHeaderLocator;
            this.setupLocator = setupLocator;
            this.dateLocator = dateLocator;
            this.heroRegions = heroRegions;
            this.playerRegions = playerRegions;
            this.castleRegions = castleRegions;
        }

        /// <summary>
        /// Returns the top level regions of the savefile, ordered by offset. Each hero, player and castle is one region whose
        /// elements are its fields, placed at their position in the savefile.
        /// </summary>
        /// <param name="document">Where the heroes, players and castles are.</param>
        /// <param name="bytes">The bytes of the savefile that go with <paramref name="document"/>.</param>
        public IReadOnlyList<ByteRegion> GetRegions(SaveDocument document, byte[] bytes)
        {
            var version = document.GameVersion();
            var regions = new List<ByteRegion>();

            AddHeader(regions, bytes);
            this.AddMapHeader(regions, bytes, version);
            this.AddSetup(regions, bytes, version);

            foreach (var hero in document.Heroes())
            {
                regions.Add(this.HeroRegion(document.Heroes().Offsets, hero));
            }

            foreach (var castle in document.Castles())
            {
                regions.Add(Group(
                    $"Castle: {castle.Name}",
                    ByteRegionKind.Other,
                    this.castleRegions.GetRegions(castle).Select(r => r.Shifted(castle.StartIndex, castle.Name)).ToList(),
                    "A castle (town) of the map"));
            }

            foreach (var player in document.Players())
            {
                regions.Add(Group(
                    $"Player: {player.Name}",
                    ByteRegionKind.Resources,
                    this.playerRegions.GetRegions(player.Layout, player.Length).Select(r => r.Shifted(player.StartIndex, player.Name)).ToList(),
                    "The resources of the colour, and what follows them"));
            }

            foreach (var obelisks in document.Obelisks())
            {
                var count = obelisks.GetByteAt(ObeliskLayout.CountOffset);
                var flags = Enumerable.Range(0, count)
                    .Select(i => new ByteRegion($"Obelisk {i + 1}", obelisks.StartIndex + ObeliskLayout.FlagsOffset + i, 1, ByteRegionKind.Game, "Visited obelisks", "1 once the obelisk has been visited"))
                    .ToList();
                regions.Add(new ByteRegion(
                    "Obelisks",
                    obelisks.StartIndex,
                    obelisks.Length,
                    ByteRegionKind.Game,
                    null,
                    "FF 00 00 and the number of obelisks of the map, then one byte for each obelisk: 1 once it has been visited. In front of the table of players",
                    [new ByteRegion("Marker", obelisks.StartIndex, ObeliskLayout.MarkerLength, ByteRegionKind.Other, "Obelisks", "FF 00 00 and the number of obelisks"), .. flags]));
            }

            var afterPlayers = document.Players().Count == 0 ? 0 : document.Players()[^1].End;
            this.AddDate(regions, bytes, version, afterPlayers);

            return regions.Where(r => r.Length > 0 && r.End <= bytes.Length).OrderBy(r => r.Start).ThenBy(r => r.Length).ToList();
        }

        private static void AddHeader(List<ByteRegion> regions, byte[] bytes)
        {
            var isChronicles = bytes.AsSpan().StartsWith(Encoding.ASCII.GetBytes(SaveHeaderLayout.ChroniclesSignature));
            if (isChronicles)
            {
                regions.Add(new ByteRegion("Signature", 0, SaveHeaderLayout.ChroniclesSignature.Length, ByteRegionKind.Header, null, "Text. Heroes Chronicles savefiles are not edited by this tool"));
                return;
            }

            if (bytes.Length < SaveHeaderLayout.MinimumLength || !(bytes.AsSpan().StartsWith(Encoding.ASCII.GetBytes(SaveHeaderLayout.Signature)) || bytes.AsSpan().StartsWith(Encoding.ASCII.GetBytes(SaveHeaderLayout.CampaignSignature))))
            {
                return;
            }

            regions.Add(new ByteRegion("Signature", 0, SaveHeaderLayout.Signature.Length, ByteRegionKind.Header, null, "Text: H3SVG, or H3SVC for a campaign"));
            regions.Add(new ByteRegion("Format version, major", SaveHeaderLayout.MajorVersion, 1, ByteRegionKind.Header, null, "Together with the minor version this tells which game version wrote the savefile"));
            regions.Add(new ByteRegion("Format version, minor", SaveHeaderLayout.MinorVersion, 1, ByteRegionKind.Header));
        }

        private static ByteRegion Group(string name, ByteRegionKind kind, IReadOnlyList<ByteRegion> parts, string description)
        {
            var start = parts.Min(p => p.Start);
            return new ByteRegion(name, start, parts.Max(p => p.End) - start, kind, null, description, parts.OrderBy(p => p.Start).ToList());
        }

        private void AddMapHeader(List<ByteRegion> regions, byte[] bytes, GameVersion version)
        {
            var location = this.mapHeaderLocator.Locate(bytes, version);
            if (location == null)
            {
                return;
            }

            const string group = "Map header";
            var parts = new List<ByteRegion>();
            if (location.SizeAt >= 0)
            {
                parts.Add(new ByteRegion("Map width", location.SizeAt, 4, ByteRegionKind.Header, group, "36 for a small map, 72 for a medium one, and so on"));
                parts.Add(new ByteRegion("Has underground", location.UndergroundAt, 1, ByteRegionKind.Header, group, "1 when the map has an underground level; the least certain of the fields of the header"));
            }

            parts.Add(new ByteRegion("Name length", location.NameLengthAt, 2, ByteRegionKind.Header, group, "The number of bytes of the name"));
            parts.Add(new ByteRegion("Name", location.NameLengthAt + 2, location.NameLength, ByteRegionKind.Header, group, "Text"));
            parts.Add(new ByteRegion("Description length", location.DescriptionLengthAt, 2, ByteRegionKind.Header, group, "The number of bytes of the description"));
            if (location.DescriptionLength > 0)
            {
                parts.Add(new ByteRegion("Description", location.DescriptionLengthAt + 2, location.DescriptionLength, ByteRegionKind.Header, group, "Text"));
            }

            parts.Add(new ByteRegion("Map difficulty", location.DifficultyAt, 1, ByteRegionKind.Header, group, "The difficulty the map was made with, from 0 for Pawn to 4 for King. Not the difficulty the game was started at"));
            regions.Add(Group(group, ByteRegionKind.Header, parts, "The name, description and difficulty of the map"));
        }

        private void AddSetup(List<ByteRegion> regions, byte[] bytes, GameVersion version)
        {
            var location = this.setupLocator.Locate(bytes, version);
            if (location == null)
            {
                return;
            }

            const string group = "Game setup";
            var colors = Enum.GetValues<PlayerColor>();
            var parts = new List<ByteRegion>
            {
                new(
                    "Player control",
                    location.ControlsAt,
                    Constants.Player.ColorCount,
                    ByteRegionKind.Game,
                    group,
                    "One byte per colour: FF when the map has no such player, 0A for a computer player, anything else for a human. It only names the first human of a hotseat game, so the human flags are used",
                    colors.Select((c, i) => new ByteRegion(c.ToString(), location.ControlsAt + i, 1, ByteRegionKind.Game, "Player control")).ToList()),
                new("Game difficulty", location.DifficultyAt, 1, ByteRegionKind.Game, group, "From 0 for Pawn to 4 for King; only a Queen game has been checked against the game"),
                new("Map file name", location.FileNameAt, location.FileNameLength, ByteRegionKind.Game, group, "Text"),
            };

            if (location.HasHumanFlags)
            {
                parts.Add(new(
                    "Human flags",
                    location.HumanFlagsAt,
                    PlayerControlLocator.HumanFlagsLength,
                    ByteRegionKind.Game,
                    group,
                    "One 32-bit integer per colour, 1 for a human",
                    colors.Select((c, i) => new ByteRegion(c.ToString(), location.HumanFlagsAt + (i * PlayerControlLocator.HumanFlagSize), PlayerControlLocator.HumanFlagSize, ByteRegionKind.Game, "Human flags")).ToList()));
                parts.Add(new("Current player", location.CurrentPlayerAt, 4, ByteRegionKind.Game, group, "The colour whose turn it was, as a 32-bit integer"));
                parts.Add(new("Marker", location.HumanFlagsAt + location.HumanFlagsToMarker, location.MarkerLength, ByteRegionKind.Other, group, "FF 10 (Horn of the Abyss) or FF 00 00 00 (the other versions), after the current player; how the human flags are found"));
            }

            regions.Add(Group(group, ByteRegionKind.Game, parts, "Who plays each colour, the difficulty of the game and whose turn it is"));
        }

        private void AddDate(List<ByteRegion> regions, byte[] bytes, GameVersion version, int searchFrom)
        {
            var at = this.dateLocator.FindOffset(bytes, version, searchFrom);
            if (at < 0)
            {
                return;
            }

            const string group = "Game date";
            var parts = new List<ByteRegion>
            {
                new("Day", at, 2, ByteRegionKind.Game, group, "Of the week, from 1"),
                new("Week", at + 2, 2, ByteRegionKind.Game, group, "Of the month, from 1"),
                new("Month", at + 4, 2, ByteRegionKind.Game, group, "From 1"),
            };
            regions.Add(Group(group, ByteRegionKind.Game, parts, "The day, week and month of the game, three 16-bit integers followed by a long run of zero bytes"));
        }

        private ByteRegion HeroRegion(HeroOffsets offsets, HeroChunk hero)
        {
            var id = hero.GetByteAt(HeroOffsets.HeroIdOffset);
            var name = $"{hero.Name} (id {id})";
            var parts = new List<ByteRegion>();
            var fields = this.heroRegions.GetRegions(offsets, hero.Length);

            if (hero.OutsidePositionBytes != null)
            {
                // The header is in front of the biography, and the start of the chunk is the end of the biography: what the
                // fields of the chunk say about the first bytes of the header is not what is there.
                parts.AddRange(this.heroRegions.GetHeaderRegions(offsets).Select(r => r.Shifted(hero.HeaderIndex, name)));
                fields = fields.Where(r => r.Start >= HeroChunk.HeaderLength).ToList();

                var biographyAt = hero.HeaderIndex + HeroChunk.HeaderLength;
                var biographyLength = hero.StartIndex + HeroChunk.HeaderLength - biographyAt;
                if (biographyLength > 0)
                {
                    parts.Add(new ByteRegion("Biography", biographyAt, biographyLength, ByteRegionKind.Identity, name, "Text"));
                }
            }

            parts.AddRange(fields.Select(r => r.Shifted(hero.StartIndex, name)));

            return Group($"Hero: {name}", ByteRegionKind.Identity, parts, hero.Biography == null ? "A hero of the map" : "A hero of the map, with a biography in front of it");
        }
    }
}
