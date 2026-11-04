# Merchant stock (HotA)

Eleven hotseat saves supplied from `Art Merch Dr Utopia`, with the companion
`merchant_stock_baseline.h3m` (original name `Art_Merch_Dra_Utopia.h3m`).
Red buys from six Black Markets, then a town Artifact Merchant; Blue checks
the shared stock and buys from the last two Black Markets. Travel and turn
changes also occur, so comparisons isolate the stock tables, not whole-save
single-action differences. Dragon Utopias on this map have not been tested.
The companion map's 96 object definitions include no mines or creature
generators; the empty mine/dwelling collections are intentional.

| Save | Original filename | Purchase or observation |
|---|---|---|
| merchant_stock_00_baseline | `GAME_BEGIN.GM2` | All eight Black Markets and the town stock untouched |
| merchant_stock_01_red_buys_gloves | `shop01.GM2` | Market 1, slot 6, Equestrian's Gloves |
| merchant_stock_02_red_buys_arrows | `shop02.GM2` | Market 2, slot 7, Angel Feather Arrows |
| merchant_stock_03_red_buys_bowstring | `shop03.GM2` | Market 3, slot 5, Bowstring of the Unicorn's Mane |
| merchant_stock_04_red_buys_orb | `shop04.GM2` | Market 4, slot 7, Orb of Tempestous Fire |
| merchant_stock_05_red_buys_pendant | `shop05.GM2` | Market 5, slot 3, Pendant of Total Recall |
| merchant_stock_06_red_buys_bow | `shop06.GM2` | Market 6, slot 2, Bow of Elven Cherrywood |
| merchant_stock_07_red_buys_town_head_of_legion | `bought_town.GM2` | Town slot 7, Head of Legion; absent in all three Red towns |
| merchant_stock_08_blue_turn | `[hotseat] 111.GM2` | Blue sees the same empty town slot and Red's purchased map slots |
| merchant_stock_09_blue_buys_boots | `shop7.GM2` | Market 7, slot 5, Boots of Speed |
| merchant_stock_10_blue_buys_armor | `shop8.GM2` | Market 8, slot 6, Armor of Wonder |

## Black Market identities

The uint32 count at decompressed `0x87A` is 8, with eight lists of seven
uint32 artifact codes starting at `0x87E`. A purchase writes `FFFFFFFF` to
one slot. Each row below is verified by its identified in-game purchase.

| Market / zero-based type-7 number | Save object index (baseline) | Map anchor (x,y,z) |
|---|---|---|
| 1 / 0 | 152 | (2,8,0) |
| 2 / 1 | 153 | (5,8,0) |
| 3 / 2 | 154 | (8,8,0) |
| 4 / 3 | 155 | (11,8,0) |
| 5 / 4 | 156 | (14,8,0) |
| 6 / 5 | 158 | (17,8,0) |
| 7 / 6 | 159 | (20,8,0) |
| 8 / 7 | 160 | (23,8,0) |

These are object anchors, not estimates from sprite edges. The companion H3M
has eight objects referencing Black Market definition 79 (`AVXmktb0.def`,
object type 7) at these anchors. The saved object list includes hero-specific
definitions on occupied objects, so do not equate its definition numbers
with H3M definition numbers. `MapPlacesLoader` resolves the saved type-7
cells' object indexes to these anchors and their numbers to stock-list indexes.

## Separate town stock

The observed town Artifact Merchant stock is shared across the three Red
towns and Blue's town: Stoic Watchman, Ladybird of Luck, Breastplate of
Petrified Wood, Helm of Chaos, Ring of Life, Red Dragon Flame Tongue,
Head of Legion. Its unique seven-uint32 sequence is at decompressed
`0x797AD` in the baseline and `0x797A5` in saves 06, 07 and 08.
Save 07 clears the last slot; save 08 retains that empty slot. Neither
changes any of the eight Black Market lists.

The stock starts **38 bytes after the saved date**: six bytes for uint16
day/week/month, then a 32-byte zero block. `TownMerchantStockLocator` uses
the uniquely marked date (`7F flag 02 flag` immediately before it), rejects
truncated/ambiguous blocks and does not search for artifact values. Thus
empty and unknown stock slots remain locatable. The stock moves to
`0x7979D` in saves 09 and 10; the locator follows it.

This structure matches 488 SoD/HotA saves in the current corpus, including
these eleven controls. The five `empire_of_the_world` / `new_day` HotA saves
have another date prefix and are deliberately refused for town stock.
Genuine RoE, AB and Chronicles formats are not enabled. Town pages with an
Artifact Merchant built now edit this single shared stock, with independent
shared reset and pending changes. Editor-written stock, neutral-town access in the game, monthly refresh and
the meaning of the surrounding fields remain unverified.

`MerchantStockTests` checks the eight purchases, all fixture round trips,
isolated edits with reload/undo/redo/revert, and the separate town-stock
observations. `MapViewModelTests` checks all eight displayed locations.
`TownMerchantStockLocatorTests` checks the moving offsets, expected stock,
the complete save corpus, unknown/sold-out slots and rejected malformed blocks.
`TownMerchantTests` checks shared Red/Blue/Neutral tabs, exact-byte edits,
reload, undo/redo/revert, independent resets and captured neutral-town discovery.
The baseline has seven towns: four Red (including `Neu_Tow`), two Neutral
(`Neu_Dung`, `Neu_Fac`) and one Blue. The two neutral towns have an unknown
`73` byte after the Mage Guild level, which must not prevent discovery or
be overwritten on capture.
