# Hota: hero owners

Hotseat autosaves of the map `Island of HOTA tests 6p` (six players, month 4), in which heroes were defeated and hired between one save and the next.
They show what the game changes when a hero changes hands: the hero's owner byte, the list of heroes of the player (bytes 0 to 9 of its record) and the table of hero owners in front of the date (`HeroOwnersLayout`).
Used by `HeroOwnerEditsTests` (the editor's owner change replayed on each pair), `HeroOwnerPickerTests` and `HeroOwnersBytesLoaderTests`.
Findings are in `docs/KNOWLEDGE_BASE.md` ("The table of hero owners").

The names given by the game were `[hotseat] 412` and so on; the number of the game is in the table.

| Save | Game's name | What it is |
|---|---|---|
| hero_owners_00_baseline | `[hotseat] 412` | Month 4, week 1, day 2. Blue has Orrin (0) and Clancy (22, selected); Red has seven heroes, Ufretin (17) among them |
| hero_owners_01_clancy_and_ufretin_defeated | `[hotseat] 413` | Day 3: Tazar (Red) defeated Clancy, and Orrin (Blue) defeated Ufretin. Blue: `[0]`, Orrin selected; Red: six heroes, the later ones moved up; the table's bytes of 17 and 22 are `FF` |
| hero_owners_02_m4w2d1 | `[hotseat] 421` | Week 2, day 1: every tavern was redrawn at the week change. Red has six heroes and offers Vokial (65) and Mephala (16) |
| hero_owners_03_red_hires_vokial__four_red_heroes_defeated | `[hotseat] 422` | Day 2: Red hired Vokial, Blue defeated Nymus (55), Damacon (84), Crag Hack (102) and Sir Mullich (144). Red: `[115, 36, 65]`; Leena (160) took Vokial's place in the tavern. Blue took a castle of Red |
| hero_owners_04_blue_hires_seven_heroes__pink_eliminated | `[hotseat] 423` | Blue hired seven heroes (its list is full at 8) and an eighth, Melodia (29), which went into the garrison of its castle; Pink lost its last town and is out (its tavern is `FF FF`) |
| hero_owners_05_seven_blue_heroes_defeated__blue_hires_seven | `[hotseat] 424` | Red defeated Blue's seven new heroes and the garrisoned one; Blue hired seven others, Sir Mullich among them. Blue: `[0, 79, 57, 48, 205, 58, 144, 127]` |
| hero_owners_06_seven_new_blue_heroes_defeated | `[hotseat] 425` | Red (Tazar) defeated the seven heroes Blue hired, Fiona (205) among them, whose byte is past the 200th of the table. Blue: `[0]`; its selected hero byte went from `FF` to Orrin, which the defeat rule does not explain (probably Orrin was clicked on the map) |
