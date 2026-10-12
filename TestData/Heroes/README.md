# Heroes

Raw hero chunks (not whole savegames), for tests of the hero layout.

| File | What it is | Used by |
|---|---|---|
| hero_maximus_pre_hota.bin | Maximus (Purple) in the shorter layout of 1072 bytes | `HeroDeserializerTests`: primary skills, army, spells, artifacts and the available spells of a hero |
| hero_maximus_hota.bin | The same kind of hero in the Horn of the Abyss layout of 1396 bytes (owner Purple, no movement points left) | `HeroDeserializerTests`, `HeroEditSessionTests` |
| hero_orrin_no_spellbook.bin, hero_orrin_with_spellbook.bin | Orrin without and with a spell book (1072 bytes each) | `HeroDeserializerTests`: the spell book flag |

The `.txt` files next to the tests in `Tests/Siniskalc.Tests/Data` hold some of the same bytes as text.
