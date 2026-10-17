# Marker and facing (HoMM2, Price of Loyalty)

Eleven saves that continue the game of Threedom (after save 82), made to find where a hero keeps the **marker the game puts on the map where it is going** (the editor calls it the destination) and **the direction it faces**. Colar Blau stands at 1, 65 in all of them.
In saves 83 to 90 he faces east and the destination goes round him, one tile at a time, clockwise from the south; in 91 to 93 he has no destination and faces west, northwest and northeast.

Result (`docs/HOMM2_SAVE_FORMAT.md`): the destination is the two 32 bit numbers at +0x17 from the name (x, y; all FF for none), and the facing the byte at +0x22, clockwise from north (0 north, 1 northeast, 2 east, 3 southeast, 4 south, 5 southwest, 6 west, 7 northwest).
Used by `HeroDestinationAndFacingTests`. The game only lists save names of at most 8 characters before the extension; the second column is the name the game gave the save.

| Save | Name in the game | What it is |
|---|---|---|
| marker_facing_83_destination_south | `83_MARKE.GX1` | The destination of Colar Blau (at 1, 65, facing east) is set one tile south of him: 1, 66 |
| marker_facing_84_destination_southeast | `84_MARK.GX1` | The destination is the tile to the southeast: 2, 66 |
| marker_facing_85_destination_east | `85_MARK.GX1` | The destination is the tile to the east: 2, 65 |
| marker_facing_86_destination_northeast | `86_MARK.GX1` | The destination is the tile to the northeast: 2, 64 |
| marker_facing_87_destination_north | `87_MAR.GX1` | The destination is the tile to the north: 1, 64 |
| marker_facing_88_destination_northwest | `88_MAR.GX1` | The destination is the tile to the northwest: 0, 64 |
| marker_facing_89_destination_west | `89_MAR.GX1` | The destination is the tile to the west: 0, 65 |
| marker_facing_90_destination_southwest | `90_MAR.GX1` | The destination is the tile to the southwest: 0, 66 |
| marker_facing_91_facing_west | `91_WEST.GX1` | Same tile, no destination, Colar Blau faces west |
| marker_facing_92_facing_northwest | `92_NW.GX1` | Same tile, no destination, facing northwest |
| marker_facing_93_facing_northeast | `93_NE.GX1` | Same tile, no destination, facing northeast |
