# Mine guardians (HoMM2, base game)

The gold mine at (43,23) is owned by Blue. Ariel (hero 24) starts with
spell power 5 and knowledge 5. Setup, including elevated spell points, was
done before the baseline. Each of the four initial casts reloads that baseline,
so those saves are independent controls rather than consecutive casts.

| Save | Name in the game | What it shows |
|---|---|---|
| mine_guardians_00_baseline | `GUARD0.GM1` | No guardian: spell FF, count 0 |
| mine_guardians_01_earth | `EARTH.GM1` | Earth spell 62, count 20 |
| mine_guardians_02_air | `AIR.GM1` | Air spell 63, count 20 |
| mine_guardians_03_fire | `FIRE.GM1` | Fire spell 64, count 20 |
| mine_guardians_04_water | `WATER.GM1` | Water spell 65, count 20 |
| mine_guardians_05_earth_spell_power_10 | `EARTH10.GM1` | Only spell power changed to 10 before casting: Earth count 40, knowledge still 5 |
| mine_guardians_06_before_orange_capture | `BEFORECAPT.GM1` | Blue's mine with 20 Earth guardians before Orange attacks |
| mine_guardians_07_orange_captures_mine | `TAKEN.GM1` | Orange owns the mine, guardian spell FF, count 0 |

The mine's seven-byte record starts at 0x568A: owner, kind, guardian spell,
guardian count, x, y, FF. Other parts of the capture pair also change; it is
not a single-action whole-save comparison.

`MineGuardianTests` checks discovery, ownership and byte-preserving round trips
and owner edits. Guardian editing is not implemented.
