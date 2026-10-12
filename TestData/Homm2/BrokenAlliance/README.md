# Broken Alliance (HoMM2, Heroes of Might and Magic II)

One game on the map "Broken Alliance" (72x72, normal difficulty), played by Blue (human, Knight), with Green (Barbarian), Red (Sorceress), Yellow (Warlock), Orange (Wizard) and Purple (Necromancer) as computer players.
The saves are in the order they were made, each differing from the one before by what the table says.
They are the base game's `.GM1`; the layout is in `docs/HOMM2_SAVE_FORMAT.md`.

**HoMM2 only lists savegames whose name has at most 8 characters before the extension.**
The repo names are longer, so the table also gives the name the game gave each save: copy a save to the game's save folder under that name.
The game also writes its own file name into the save, at offset `0x29E`; that is why the first bytes of the saves differ.

## The start (save 01)

- Blue: 20 wood, 5 sulfur, 5 crystal, 5 mercury, 20 ore, 5 gems, 7500 gold.
- Town Xabran (a castle, with only dwelling 1, the Thatched Hut). The tavern offers Sir Gallant (Knight) and Roxana (Necromancer).
- Hero Maximus: attack 2, defense 2, spell power 1, knowledge 1; level 1, 0 experience; secondary skills Leadership basic and Ballistics basic; no spell book and no artifacts (HoMM2 has no backpack); 37 peasants and 4 archers in slots 1 and 2 (an army has 5 slots).

## Saves

| Save | Name in the game | What happened | Used by |
|---|---|---|---|
| broken_alliance_01_start | `1_START.GM1` | The start of the map: Blue (Knight) has Maximus in Xabran, which has only dwelling 1; 20 wood, 5 mercury, 20 ore, 5 sulfur, 5 crystal, 5 gems, 7500 gold; the tavern offers Sir Gallant and Roxana | |
| broken_alliance_02_maximus_split_army | `2_SPLIT.GM1` | Maximus splits his army: 34 peasants, 4 archers and three stacks of 1 peasant | |
| broken_alliance_03_maximus_move | `3_MOVE.GM1` | Maximus moves 1 square | |
| broken_alliance_04_maximus_collects_gold | `4_GOLD.GM1` | Maximus collects two 600-gold treasures: Blue has 8700 gold | |
| broken_alliance_05_xabran_archery_range | `5_ARCHRA.GM1` | Maximus moves until he has no movement points left; Xabran builds the Archery Range (dwelling 2): 7700 gold | |
| broken_alliance_06_day_2_sawmill_xabran_mage_guild_1 | `6_NTURN.GM1` | New day (day 2): Maximus takes 2 stacks of wood (+16) and captures a sawmill; Xabran builds the Mage Guild level 1 | |
| broken_alliance_07_maximus_in_xabran | `7_GARRIS.GM1` | Day 3: Maximus goes into Xabran (only a visitor, there are no garrisons in HoMM2) | |
| broken_alliance_08_sir_gallant_hired | `8_NEWHER.GM1` | Blue hires Sir Gallant, who is in Xabran | |
| broken_alliance_09_sir_gallant_level_up | `9_LVLUP.GM1` | Sir Gallant levels up (level 2: defense +1 and the secondary skill Pathfinding) | |
| broken_alliance_10_xabran_mage_guild_2__maximus_spell_book | `10_GUILD.GM1` | Xabran builds the Mage Guild level 2 (level 1: Magic Arrow, View Resources, Cure; level 2: Disrupting Ray, View Artifacts, Steelskin); Maximus buys the spell book | |
| broken_alliance_11_xabran_statue__sir_gallant_medal_of_courage | `11_ART.GM1` | Xabran builds the Statue; Sir Gallant picks up a Medal of Courage | |
| broken_alliance_12_new_week_xabran_well | `12_NEWWE.GM1` | New week (week 2); Xabran has built the Well, so its dwellings grow by 2 more | |
| broken_alliance_13_maximus_fountain__xabran_tavern_thieves_guild | `13_FOUNT.GM1` | Maximus visits a fountain, and his troops change to 5 black dragons; Xabran has the Tavern and the Thieves Guild by now (they were built after save 12) | |
| broken_alliance_14_corackston_captured | `14_NEWC.GM1` | Blue captures Corackston (neutral, Wizard), a town with no castle and only dwelling 1; without a castle nothing else can be built | |
| broken_alliance_15_maximus_ammo_cart | `15_ART2.GM1` | Maximus gets an Ammo Cart | |
| broken_alliance_16_sir_gallant_defeats_natasha | `16_NAT.GM1` | Sir Gallant defeats Natasha (Red) and gains her artifact, the Ice Cloak; Natasha spent spell points in the battle | |
| broken_alliance_17_maximus_magic_arrow | `17_MARRO.GM1` | Maximus fights and casts Magic Arrow | |
| broken_alliance_18_corackston_lost_to_yellow | `18_LOSTC.GM1` | Corackston is lost to Yellow | |
| broken_alliance_19_whiteshield_captured | `19_CAST.GM1` | Blue captures Whiteshield (Warlock) | |
| broken_alliance_20_whiteshield_shipyard | `20_SHIPY.GM1` | Whiteshield builds the Shipyard | |
| broken_alliance_21_blue_has_no_castle | `21_NOCAS.GM1` | Blue is left without a castle: the 7-day countdown starts. Green is about to be eliminated for the same reason | |
| broken_alliance_22_blue_4_days_left | `22_4DAYS.GM1` | 4 days are left for Blue; Green was kicked out of the game | |
| broken_alliance_23_blue_3_days_left | `23_3DAYS.GM1` | 3 days are left for Blue | |
| broken_alliance_24_castles_recaptured__whiteshield_green_tower | `24_GREEN.GM1` | Blue captures castles again (including Corackston, which Yellow took) and builds the Green Tower (the Green Dragon dwelling) in Whiteshield | |
