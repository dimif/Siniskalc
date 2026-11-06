# Disabled heroes (HotA)

One hotseat save of the map used for the creature-bank trials (`games\Creature Banks\[hotseat] ac.GM1`), in which the map editor disabled these heroes:
Gelu, Giselle, Dracon, Xeron, Haart Lich, Mutare, Mutare Drake, Kilgor, Boragus, Adrienne, Kinkeria, Derek, Bidley, Tark, Tavin, Murdoch, Frederick and Biarma.
Their entry in the table of hero owners is reserved (`40`), as the entry of a hero a tavern offers is, but no tavern offers them.
Catherine, Roland (campaign ids 146 and 152) and Beatrice (175) have the same entry and were not on the list given.

| Save | Original filename | What it is |
|---|---|---|
| disabled_heroes_00_baseline | `[hotseat] ac.GM1` | Red and Blue; 21 heroes are reserved and in no tavern, 4 are reserved and in a tavern (Iona, Rashka, Broghild, Wrathmont) |
| disabled_heroes_01_adrienne_hired_and_retreated | `adrienne_retreated.GM2` | Made from `00`: Adrienne's entry was changed from reserved to free by hand (one byte) and she was put in the first place of Red's tavern with the editor (Red: 145, 119); in the game Red hired her, fought and retreated. She is back in Red's tavern with the reserved entry (the game writes it), still on the map tile she retreated from (38, 5) with one Gnoll |

The editor does not write the reserved entry when it puts a hero in a tavern, so the save made by the editor alone (Adrienne in the tavern, entry free) is not kept here: `EverySavefileHasATableThatAgreesWithTheHeroesAndThePlayers` would reject it. The game hired her from that state without complaint.
Hiring a hero that the map disables worked in the game once the editor had put her in a tavern, so the game does not refuse a campaign hero. Whether the game draws such a hero for a tavern by itself is not known: after about 134 hires and dismissals (335k gold at 2.5k each) in a pool of 188 free heroes Adrienne had not been offered, which is what a chance of about one half would give if she could be drawn.

Used by `DisabledHeroesNavigationTests`.
