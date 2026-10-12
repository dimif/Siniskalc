# Hota: sleeping, garrisoned and visiting heroes

Red's castle Ellesmere with the heroes Akka (id 211) and Oidana (id 199).
Used by `HotaSleepGarrisonVisitingTests`.
Findings are in `docs/KNOWLEDGE_BASE.md` ("Heroes", garrison, visit and swap).

| Save | State | What it checks |
|---|---|---|
| sleep_garrison_visiting_01_akka_visiting | Akka visits the castle | Visiting id in the castle, hero byte 6 = 1; Red is not in the player table (no resources yet) |
| sleep_garrison_visiting_02_akka_garrisoned | Akka is garrisoned | Garrisoned id in the castle, byte 6 = 0, player hero list does not hold her |
| sleep_garrison_visiting_03_akka_asleep | Akka visits again and is asleep | Asleep flag at +1087 in the HotA chunk |
| sleep_garrison_visiting_04_akka_garrisoned_oidana_visiting | Akka garrisoned (still asleep), Oidana visits | Both slots; player list holds Oidana only; Red appears in the table |
| sleep_garrison_visiting_05_oidana_not_visiting | Oidana has moved away | Moved-today flag (+1095) set for Oidana; the castle visiting slot empties |
| sleep_garrison_visiting_06_garrison_visit_swap1 | Oidana back at the castle, the heroes swap | Castle ids exchanged, both hero byte 6 flipped, player list now Akka |
| sleep_garrison_visiting_07_garrison_visit_swap2 | Swap again | Only byte 6 of Akka and byte 6 plus facing (41) of Oidana change; player list back to Oidana |
| sleep_garrison_visiting_08_oidana_asleep | Oidana has moved away and is asleep | Asleep flag for a second hero |
