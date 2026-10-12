# Arrax the Jerk (HoMM2, Price of Loyalty)

One save of the expansion map "Arrax the Jerk" (36x36, 53,820 bytes), made in the Price of Loyalty, so it has the `.GX1` extension, a 4-byte marker at the start of the file and 250-byte hero records (see `docs/HOMM2_SAVE_FORMAT.md`, "Game versions").
Blue is a Barbarian and starts with no resources; the hero is Bikko (hero 16). The enemy is Arrax (Green): the hero King Anthrax and the castle Arrax.

| Save | Name in the game | What it is | Used by |
|---|---|---|---|
| arrax_the_jerk_01_start | `1_EXP_MA.GX1` | The start of the map | `Homm2SavegameParserTests`, `Homm2SaveLayoutLocatorTests`, `Homm2EditingTests` (the 14 extra bytes of a hero), `ShellGamesTests` |

The game only lists savegame names with at most 8 characters before the extension; the second column is the name the game gave the save.
