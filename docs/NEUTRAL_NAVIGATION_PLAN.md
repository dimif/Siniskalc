# Plan: mines and dwellings under their owner, and a page for Neutral

Status: proposal, nothing built. Mocks were shown in the session (tree, a player's Mines tab, the Neutral page).

## What changes for the user

1. A player (colour) that owns at least one mine gets a **Mines** tab and a **Mines** node under its node. Same for **Dwellings**.
2. The neutral mines and dwellings are the same two nodes under **Neutral**, no longer tabs of **Map**.
3. **Neutral** becomes a page with tabs **Heroes, Towns, Mines, Dwellings** (a player's page without General, Obelisks and Hex; see "No Hex tab for Neutral").
4. **Map** keeps what is not owned by anyone: merchants (HoMM3); shrines and sphinxes (HoMM2).

```
Game
├ Red  · 2 heroes · 3 towns · 4 mines · 2 dwellings
│  ├ Heroes ▸ Towns ▸ Mines (4) ▸ Dwellings (2)
├ Blue · 1 hero · 1 town            (owns no mine: no Mines node)
├ Neutral (page)
│  ├ Heroes ▸ Towns ▸ Mines ▸ Dwellings
└ Map  (merchants / shrines / sphinxes)
```

## Both games?

CLAUDE.md says every rule applies to both games unless it cannot. What was found:

- **Mines**: both games have `IMineModel<PlayerColor>` with an owner. Both get the change.
- **Dwellings**: HoMM3 only. HoMM2's `DwellingRowViewModel` is a dwelling of a *castle*; the HoMM2 map has no owned map dwellings. HoMM2 gets no Dwellings tab or node (the shared code takes "has dwellings" from the game, so a game with none shows none).
- **Neutral page**: both games; HoMM2 has no Dwellings tab.

## How the pieces fit today

- `OwnerNavigationBuilderBase` builds one node per owner through `OwnerNavGroup.Create` (Heroes and Towns groups only). `OwnerNavGroup` makes a `Group` node when the owner has no page (Neutral).
- A player's page is `PlayerViewModel` with `SelectedTab` 0 General, 1 Heroes, 2 Towns, 3 Obelisks, 4 Hex. `PlayerPage.xaml` hard-codes `Value="4"` for Hex in a trigger.
- `MapViewModel` (each game) builds a row for **every** mine and dwelling in the save, with `MinesListViewModel<TRow>` and `SortableListViewModel`.
- The tree is rebuilt when `StructureKey` changes; it only reads castle and hero owners now.

## Design

### 1. Owner filter over the rows that exist once

Build the mine and dwelling rows once per game, not per page, and give each page a filtered, sorted view of them.

- New `MapObjects` holder per game (HoMM3: mines, dwellings, merchants rows; HoMM2: mines, shrines, sphinxes). `MapViewModel` takes it instead of creating rows. It is built by `SaveDocumentFactory`/the editor view model with the other per-savefile things, not by a page.
- `MinesListViewModel<TRow>` gets an owner filter (`Func<TRow,bool>`, refreshed in `Refresh()`); the HoMM3 dwelling list gets the same so Core's `SortableListViewModel` is reused unchanged if it can filter, otherwise it gets a `Filter` property (Core, no game types).
- Rows stay `MineRowViewModel`, `MapDwellingRowViewModel`; the owner combobox stays on the row, so changing an owner removes the row from that page on the next `Refresh`.
- A reducer is not needed: owners are already edited through the sessions of the records.

### 2. Navigation

- `NavNodeKind` gets `Mine`, `Dwelling` (leaf entries), `OwnerSubGroup` (resource / faction) and `Neutral`, `NeutralTab`. Prefer one `OwnerTab` over `PlayerTab` + `NeutralTab` if the editors can switch on the owner; decide when writing step 2.
- **Mines and dwellings are grouped one level deeper** (`Red → Mines → Gold → <each gold mine>`, `Red → Dwellings → Castle → <each Castle dwelling>`; the same under Neutral):
  - Mines are grouped by the resource they give. `MineKind` has seven values, all of which give a resource (Sawmill/Wood, Alchemist's lab/Mercury, Ore, Sulfur, Crystal, Gems, Gold). The group title is the resource (shared `Resource` name), the count is the number of mines; groups are in the order of the game's resources, and an empty group is not made.
  - Dwellings are grouped by the faction of the creature they give (`DwellingModel.Creature.Faction`: Castle, Rampart, ..., Neutral last). A dwelling whose creature code is unknown (`Creature` is `null`) goes to an **Unknown** group at the end, never lost. HoMM3 only.
  - A leaf is titled like the row today (`Gold mine at 12, 7`; `Griffin Tower at 5, 9`) with the level as the subtitle.
  - `OwnerNavGroup` takes a list of `NavGroup(Title, Entries)` per section instead of a flat list; Heroes and Towns pass one group with no sub-title (unchanged: no extra level). A section with a single sub-group still shows it, so the tree has the same shape for every owner.
  - Selecting a resource or faction group shows the Mines (or Dwellings) tab of the owner, unfiltered. These sub-group nodes are **not** registered as tab nodes (`playerTabNodes` is keyed by `(player, tab)` and the "Mines" node must stay the one registered), so they get a new `NavNodeKind.OwnerSubGroup`.
  - The group nodes of an owner with ~58 neutral dwellings keep the tree short: Neutral → Dwellings → 9 faction groups, each collapsed at first.
- `OwnerNavGroup.Create` gets the `mines` and `dwellings` sections (as `NavGroup` lists) and adds them only when non-empty (as Heroes/Towns). Tab numbers become named constants (`OwnerTabs.General = 0 ... Hex`), not literals in XAML triggers.
- `OwnerNavigationBuilderBase` gets abstract/virtual `MineGroupsOf(owner)` and `DwellingGroupsOf(owner)`; HoMM2 returns an empty dwellings list. The grouping by resource is shared (`IMineModel.Kind`); the grouping by faction is HoMM3's.
- `StructureKey` adds the mine and dwelling owners so the tree is rebuilt when one changes hands.
- `NavigationTree` registers mine and dwelling nodes by index; `MarkChanges` already marks by `(ChunkKind, Index)`: add `Mine -> Mines` and `Dwelling -> Dwellings` to each game's `Kinds` dictionary so unsaved edits show on the node.
- `Map` stays, with the summary of what is left; it is hidden when there is nothing left (the existing "no node when empty" rule).
- Picking a mine or dwelling node shows its owner's page on the Mines/Dwellings tab with that row selected (`SelectedMine`).

### 3. Pages

- Player pages: add **Mines** and **Dwellings** tabs. New `PlayerPage` tab order: General, Heroes, Towns, Mines, Dwellings, Obelisks, Hex; Hex index moves from 4 to 6 (use the constants). The tabs are visible only when the player owns any.
- Neutral: new `NeutralViewModel` (`Siniskalc.Heroes.Wpf`) bound through an interface (`INeutralPage`, like `IPlayerPage`) and a `NeutralPage.xaml` per game made of existing blocks (`ItemsCard`, `MinesCard`, `TavernCard` not needed). HoMM3 reuses the dwelling template from `MapPage.xaml` (move it to a shared block `DwellingsCard` in `Heroes.Wpf` only if HoMM2 will ever use it; otherwise leave it in `Homm3/Views`).
- The guard editor below the mines list moves with the mines list to both the player and the Neutral page.
- **No Hex tab for Neutral.** The hex editor edits one chunk through an `IByteEditTarget` (`HeroByteEditTarget`, `PlayerByteEditTarget`, `CastleByteEditTarget`; no target exists for mines or dwellings, which are records in a table). A player has one chunk, so its Hex tab makes sense. Neutral is ~40 hero chunks, its town chunks and scattered mine/dwelling records, so there is no byte range to show and a picker would only repeat the Hex tab each hero and town page already has. Neutral heroes and towns keep their own Hex tab on their own page; the whole file stays in *Advanced > View save hex*. Neutral tabs are therefore Heroes, Towns, Mines, Dwellings.
- **Hex next to the entity instead** (new). Mines and dwellings are `RecordChunk`s (small `BytesChunk`s) with edit sessions (`CreateMineSession(i)`, `CreateDwellingSession(i)`), the same shape as `CastleEditSession`, so they can have a byte edit target like a hero, town or player:
  - `MineByteEditTarget` and `DwellingByteEditTarget` (`ChunkByteEditTarget<RecordChunk, MineModel>` / `<..., DwellingModel>`), with field regions (kind, owner, position, guard / creature, stock), and two methods on `IByteEditTargetFactory`: `ForMine`, `ForDwelling`. HoMM2 gets `ForMine` only.
  - **Where it shows:** a **Hex** card under the picked mine, below its guard editor (mines tab), and under the picked dwelling (dwellings tab), advanced mode only (`RequiresAdvancedMode`, as the Hex tabs). It follows `SelectedMine` / the picked dwelling row and is disposed and rebuilt when the pick changes (the factory's caller disposes the target).
  - It works the same on a player's Mines tab and on the Neutral page, since both use the same mines card.
  - Dwellings need a "picked dwelling" like `SelectedMine`: add it to the HoMM3 dwelling list view model.
  - The whole file stays in *Advanced > View save hex*.

### 4. What to remove

- The Mines and Dwellings tabs of `MapPage` (HoMM3) and the mines tab of the HoMM2 map page; `MapViewModel.MapSummary` and `GuardHeading` move with them. Merchants (HoMM3), shrines and sphinxes (HoMM2) stay.

## Steps (each ends with the whole suite green)

1. **Rows once, filtered**: `MapObjects` per game, owner filter on the mines and dwellings lists. `MapViewModel` still shows everything (no UI change yet). Tests: filter by owner, owner change moves a row, sort survives a filter.
2. **Nav entries**: new node kinds, `OwnerNavGroup` Mines/Dwellings groups, builder hooks, `StructureKey`, chunk kinds for change marks. Tests: `OwnerNavGroupTests`, both `NavigationBuilder` tests (player with/without mines, HoMM2 has no dwellings node, change of owner rebuilds).
3. **Player pages**: Mines and Dwellings tabs, tab constants, selecting a mine node selects the row. Rendering tests (`PageRenderingTests`) check the tabs show their rows, since a template that failed to apply raises no binding error.
4. **Neutral page**: `NeutralViewModel`, `INeutralPage`, a page per game, node kind `Neutral`. Tests: shape contract in `PageContractTests`, rendering.
4b. **Hex per record**: `MineByteEditTarget`, `DwellingByteEditTarget`, factory methods, a `SelectedDwelling`, and the Hex card under the picked mine and dwelling. Needs step 3 (the cards in their new place); the targets themselves can be built and tested earlier. Tests: an edit through the target is one undoable change and marks the node; the target is rebuilt on a new pick.
5. **Map cleanup**: remove the moved tabs, update the summary and the empty-node rule.
6. **Docs**: `docs/USER_GUIDE.md` (where mines moved), `docs/SINISKALC_ARCHITECTURE.md`, and the test counts if any are listed.

Steps 1 and 2 are independent of each other in files and can run in parallel; 3 needs both; 4 needs 1 and 2; 5 needs 3 and 4.

## Risks

- A tree with ~60 neutral dwellings and ~40 neutral heroes: keep the Neutral node collapsed at first (already `DefaultExpanded = owned`).
- Rebuilding the tree on every owner edit may lose the selection or the expanded state: `Reveal` and `IsExpanded` need to survive it (check what happens today when a castle changes owner).
- Moving the Hex index from 4 to 6 breaks any saved tab index and the hex trigger in the XAML; tests that select tab 4 need the constant.
- The Dwellings tab of the Map is marked "Not yet tried in the game" in its text; keep that note on the moved tabs.
