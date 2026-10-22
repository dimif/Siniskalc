# What can move into Siniskalc.Heroes(.Wpf): review of `ViewModels` and `Views`

**2026-10-20:** the resources of a player (a row each, the "All" and "Gold" buttons, the limits) are `ResourcesEditorViewModel`, `ResourceRowViewModel` and `ResourceLimits` in `Siniskalc.Heroes.Wpf`; both `PlayerViewModel`s hold one and pass their own limits. A line-by-line comparison of the two games' view models found no other identical code: what is left (the hero, castle and player pages) differs in structure, not text. The overview page of each game derives from `OverviewViewModelBase` (file name, map, date, difficulty, turn and the summary line); a game says where its facts are (`Map`, `Date`, `Difficulty`, `CurrentPlayerName`, nullable) and builds its cards.

**2026-10-21:** a second pass over both games.
Done: the resource, tavern and days-left changes of a player are `AddResources`, `AddTavern` and `AddDaysLeft` in `ChangeSinkExtensions` (HoMM3 now reports a change of the days left, which it did not);
`OwnedLinks.Heroes`, `HeroLinkViewModel` and `OwnerNavigationBuilderBase.HeroEntriesOf` are the hero lists of the overview, the player page and the navigation, and `CastleLinks.OwnedBy` is the castle list of each game;
`SessionPageViewModelBase<TModel>` (over `IEditSession<TModel>`) holds the pending changes and the reset of the HoMM3 pages;
`HeroesArmyViewModelBase<TCreature, TSlot>` is the army editor, `HeroesSecondarySkillsViewModel<TSkill, TSkills>` is the skills editor (`ISecondarySkills.WithSkillLevel` handles the full hero in the model), and `SkillLevelNames` moved to `Siniskalc.Heroes.Wpf`;
`IGameInfo<TColor>` is implemented by both `Game` classes, and `OverviewViewModelBase<TColor>` reads the date, map, difficulty and turn from it.
**2026-10-22:** the obelisks card of the player page is one `ObeliskVisitsViewModel<TColor>` in `Siniskalc.Heroes.Wpf` (over `ObeliskVisitsViewModelBase`); the two games' copies are gone. A game gives the kind of chunk of its obelisks, the word for what the puzzle leads to ("Ultimate Artifact", "Grail") and how it writes a place (HoMM3 with the level, HoMM2 without).

Not done (2026-10-21): a shared player page base (HoMM2's `PageViewModel` and HoMM3's `SessionPageViewModelBase` are different bases, so a third one cannot sit between them; the shared blocks are already `ResourcesEditorViewModel`, `TavernPlaceViewModel` and `ObeliskVisitsViewModelBase`), and HoMM2 pages on the session base (its owner changes touch two chunks, so its reset reverts pending changes of the page).

The same kind of review as `SHARING_REVIEW.md`, for the view models and views of `Siniskalc.Homm2` and `Siniskalc.Homm3`, read on 2026-10-17.
Verdicts are the same: SHARE, GENERIC, INTERFACE, KEEP. **Done** marks what was changed with this review.

## What is already shared

`Siniskalc.Wpf`: the shell, the navigation tree, `EditorViewModelBase`, the army, secondary skill and spell view models and views (`ArmyViewModelBase`, `SecondarySkillsViewModelBase`, `SpellsViewModelBase`, `FinderViewModel`), the artifact picker, steppers, hex editor.
`Siniskalc.Heroes(.Wpf)`: the page blocks (`PageHeader`, `ResourcesPanel`, `ItemsCard`, `OwnerPicker`, `HeadedCard`), the page interfaces (`IPlayerPage`, `ICastlePage`, `IResourcesPage`, `IResettablePage`), `OwnerNavGroup`, `NavEntry`, `CreatureFinderViewModel`, the palette.

## Summary

| # | Component | HoMM2 | HoMM3 | Verdict | Status |
|---|---|---|---|---|---|
| V1 | Overview sentences ("Difficulty: Queen (160%)", "2 heroes") | written in place | written in place | SHARE (`OverviewText`) | **done** |
| V2 | Place on the map ("X: 28, Y: 27") | `ToString` of the tile | `MapLevelText` | SHARE (`MapLevelText` in `Heroes.ViewModels`) | **done** |
| V3 | Spell rows and finder | name, cost | name, school, level, type, availability | KEEP the finder; rows now alike | **done** (HoMM2 shows level, type and mind/elemental) |
| V4 | Page base: tab, pending changes, Reset, dispose | `PageViewModel` | repeated in each page | GENERIC base in `Heroes.Wpf` | **done** (`ResettablePageViewModelBase`) |
| V5 | Navigation builder | 115 lines | 166 lines | GENERIC base | **done** (`OwnerNavigationBuilderBase`; the games supply owners, groups and marks) |
| V6 | Player page: resources, quick buttons | `SetAll`, `SetRich` | the same, plus tavern, days left, obelisks | INTERFACE (`IResourcesPage`) already; share tavern | tavern **done** (`TavernCard`, `TavernPlaceViewModel`, `TavernChoices`; HoMM2 edits it now); days left and obelisks open |
| V7 | Overview page | list of players | cards with heroes and castles | SHARE (`OverviewView`, `IOverviewPage`, `PlayerCardViewModel`, `CastleLinkViewModel`) | **done**; HoMM2 has the HoMM3 cards. Not shown for HoMM2 until decoded: who controls a colour, whose turn it is |
| V8 | Castle page: dwellings, Mage Guild | `DwellingRowViewModel`, `GuildLevelViewModel` | `DwellingRowViewModel`, `MageGuildLevelRowViewModel` | wait for M12 | open |
| V9 | Hero page | 4 sections | 6 sections and a General tab | KEEP; owner picker shared | owner picker **done** for HoMM2 (the hero joins the list of heroes of its player and a neutral one has no move to finish, as in Broken Alliance saves 15 to 16); `IHeroPage` open |
| V10 | Artifact choice and finder | bonuses | effect lines, slot, spell | KEEP; share the bonus text | **done** (`PrimarySkillBonusText`; HoMM2 now says "Attack +6" as HoMM3 does) |
| V11 | Skill level text | `SkillLevelText` (Wpf) | `SkillLevelNames` | follows E3 | deferred |

## Done

- **V1 `OverviewText`** (`Heroes.ViewModels`): `Difficulty`, `MapRating`, `Count`. The difficulty and rating lines were written three times, the "1 hero / 2 heroes" test five times.
- **V2 `MapLevelText`** moved from HoMM3 to `Heroes.ViewModels`; HoMM2's hero and castle pages now say "X: 28, Y: 27" as HoMM3's say "X: 33, Y: 23, Surface (0)" (a game with one level leaves the level out).
- **V3**: HoMM2 spell rows carry the level and, from `SpellGroup`, "Mind" or "Elemental"; the finder has Level, Type and Magic columns and searches them ("level 3", "mind", "combat").
  `ISpell` (`Type`, `Level`) is in `Heroes.Entities` and both `Spell` classes implement it.
  **The levels of the HoMM2 spells are from the fheroes2 tables and are not confirmed.** I compared them with the Mage Guilds in the test saves and they disagree for 17 spells (a guild row does not always hold spells of its own level, so the saves cannot settle it). Please check them in the game.

## Open

- **V4 page base.** HoMM2 has one `PageViewModel` for hero, castle and player (tab, `HasPendingChanges`, `ResetCommand`, dispose, hex target). HoMM3 repeats the same four members in `HeroViewModel`, `CastleViewModel`, `PlayerViewModel`. A base in `Heroes.Wpf` that takes "the pending changes of this page" and "revert them" as delegates would serve both without knowing `Game` or `PlayerEditSession`. Medium work, no behaviour change; the best next step.
- **V5 navigation builder.** The skeleton is the same: the overview node, the list of owners (players with data, plus owners of heroes and castles, plus "nobody"), `StructureKey`, the group per owner through `OwnerNavGroup`. What differs is real: HoMM3's badges (capital, grail, garrison) and the countdown notes, HoMM2's visitor name. A generic `OwnerNavigationBuilderBase<TGame, TColor>` with the owners, entries and marks supplied by the game would remove about 60 lines each; it needs a small interface for "the game's history" (both `Game`s have one, of different types).
- **V6/V7 feature gaps** that sharing would close: HoMM2's player already has a tavern model and no tavern card; HoMM2's overview is a plain list while HoMM3's shows each player's heroes and castles. The data is there in both games; the cards (`TavernPlaceViewModel`, `PlayerCardViewModel`, `HeroLinkViewModel`) would have to take interfaces (`ILink`, `TavernHeroes`) instead of HoMM3 types.
- **V8** waits for the shared castle values (M12 in `SHARING_REVIEW.md`).
- **V9** hero page: HoMM3 is the superset (General tab, backpack, war machines); the four sections they share already come from `Siniskalc.Wpf`. An `IHeroPage` for the header and Reset, as for the player and castle pages, is the only step left.
- **V10**: HoMM2's `Describe(PrimarySkillBonus)` can serve HoMM3's effect lines for the primary skills through `IArtifact`; small.

## Views (XAML)

The two `Generic.xaml` files (16 and 99 lines) hold only styles a game's own pages use. `HeroPage` (114 and 95 lines) and the artifact views differ by the layout of the slots. The `PlayerPage`, `CastlePage` and `OverviewPage` of HoMM3 are 3 to 5 times longer than HoMM2's because of the extra cards, not because of copied markup; the repeated markup (headers, resources, owner picker, items cards) already lives in the blocks of `Siniskalc.Heroes.Wpf`. No XAML is worth moving before V4 to V7 are done.
