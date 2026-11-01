import json
import pathlib
import copy

root = pathlib.Path("Siniskalc.Homm3") / "GameData"
base = root / "Baseline"


def camel(value):
    if isinstance(value, dict):
        return {key[0].lower() + key[1:]: camel(item) for key, item in value.items()}
    if isinstance(value, list):
        return [camel(item) for item in value]
    return value


def save(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


domains = {}
for old, new in [
    ("Faction", "factions"), ("CreatureAbility", "abilities"), ("Building", "buildings"),
    ("Creature", "creatures"), ("Spell", "spells"), ("HeroClass", "heroClasses"),
    ("SecondarySkill", "secondarySkills"), ("WarMachine", "warMachines"), ("Artifact", "artifacts"),
]:
    domains[new] = camel(json.loads((base / (old + ".json")).read_text()))
for d in domains["factions"]:
    d["isNeutral"] = d["id"] == "Neutral"
    d["companionBits"] = (
        "halls" if d["id"] in ["Rampart", "Necropolis"] else
        "fort" if d["id"] in ["Bulwark", "Factory", "Stronghold"] else
        "fortAndHalls" if d["id"] == "Conflux" else "none"
    )
for d in domains["buildings"]:
    d["automatic"] = d["faction"] is not None and d["code"] in [19, 25]
    d["goldIncome"] = {"VillageHall": 500, "TownHall": 1000, "CityHall": 2000, "Capitol": 4000}.get(d["id"], 0)
codeids = {d["code"]: d["id"] for d in domains["creatures"]}
hotacreatures = []
for d in domains["creatures"]:
    d["fallback"] = d["id"] == "Peasant"
    code = d.pop("baseCreatureCode")
    d["baseCreature"] = codeids[code] if code is not None else None
    variants = d.pop("versionInfo")
    d["info"]["special"] = d["info"]["special"] or []
    if variants:
        h = copy.deepcopy(d)
        h["info"] = variants["hornOfTheAbyss"]
        h["info"]["special"] = h["info"]["special"] or []
        h["replaces"] = True
        hotacreatures.append(h)
for d in domains["spells"]:
    d["heroOnly"] = not d.pop("canBeInMageGuild")
for d in domains["heroClasses"]:
    d["fallback"] = d["id"] == "Knight"
for d in domains["secondarySkills"]:
    d["role"] = {"Leadership": "morale", "Luck": "luck", "Intelligence": "spellPointMultiplier"}.get(d["id"], "none")
for d in domains["warMachines"]:
    d["encoding"] = d["id"][0].lower() + d["id"][1:]
for d in domains["artifacts"]:
    d["role"] = {"SpellScroll": "scroll", "SpellBook": "spellBook"}.get(d["id"], "none")
    d["wearable"] = d["id"] not in ["SpellBook", "Catapult", "TheGrail"]
    d["backpack"] = d["wearable"] or d["id"] == "TheGrail"
    effect = d["effectInfo"]
    effect["other"] = effect["other"] or ""
    effect["spellGrants"] = effect["spellGrants"] or []
    for grant in effect["spellGrants"]:
        grant.setdefault("spells", [])
        grant.setdefault("school", None)
        grant.setdefault("level", None)
        if grant["school"]:
            grant["school"] = grant["school"][0].lower() + grant["school"][1:]
domains["spellSchools"] = [
    dict(id=name, order=i, name="Air" if name == "wind" else name.title(), code=i,
         availableFrom="restorationOfErathia", description={"all": "All schools", "wind": "Air magic"}.get(name, name.title() + " magic"),
         universal=name == "all")
    for i, name in enumerate(["water", "earth", "fire", "wind", "all"])
]
slots = ["experience", "defense", "knowledge", "attack", "power", "level", "primarySkills", "attackOrDefense", "powerOrKnowledge", "paidAttackOrDefense", "hotaPrimarySkills", "hotaExperience"]
domains["visitedPlaces"] = []
for p in camel(json.loads((base / "VisitedPlaceKind.json").read_text())):
    p.pop("heroOffset")
    p["code"] = p["order"]
    p["slot"] = slots[p["order"]]
    p["availableFrom"] = "hornOfTheAbyss" if p["order"] >= 10 else "restorationOfErathia"
    domains["visitedPlaces"].append(p)
for name, data in domains.items():
    save(root / "standard" / (name + ".json"), data)
    save(root / "hota" / (name + ".json"), hotacreatures if name == "creatures" else [])
save(root / "manifest.json", dict(
    schemaVersion=1,
    versions={v: ("hota" if v == "hornOfTheAbyss" else "standard")
              for v in ["restorationOfErathia", "armageddonsBlade", "shadowOfDeath", "heroesChronicles", "hornOfTheAbyss"]},
    sets={"standard": dict(parent=None, directory="standard"), "hota": dict(parent="standard", directory="hota")}))
save(root / "mapObjects.json", dict(
    merchantName="Black market", dwellingName="dwelling",
    mines=[dict(kind=kind, name=name, resource=resource, dailyAmount=amount, fallback=kind == "goldMine")
           for kind, name, resource, amount in [
               ("sawmill", "Sawmill", "wood", 2), ("alchemistLab", "Alchemist's lab", "mercury", 1),
               ("oreMine", "Ore mine", "ore", 2), ("sulfurMine", "Sulfur mine", "sulfur", 1),
               ("crystalMine", "Crystal mine", "crystal", 1), ("gemMine", "Gem mine", "gems", 1),
               ("goldMine", "Gold mine", "gold", 1000)]],
    visitEffects=[dict(effect=effect, order=i, name=name, description=description, morale=morale, luck=luck, editable=editable)
                  for i, (effect, name, description, morale, luck, editable) in enumerate([
                      ("wateringHole", "Watering hole", "+1 morale until the next battle", 1, 0, True),
                      ("swanPond", "Swan pond", "+2 luck until the next battle", 0, 2, True),
                      ("crypt", "Crypt", "-1 morale until the next battle", -1, 0, True),
                      ("stables", "Stables", "More movement points until the end of the week", 0, 0, True),
                      ("faerieRing", "Faerie ring", "+1 luck until the next battle", 0, 0, False),
                      ("fountainOfFortune", "Fountain of fortune", "A luck that the game draws, until the next battle", 0, 0, False),
                      ("idolOfFortune", "Idol of fortune", "+1 morale or +1 luck, depending on the day", 0, 0, False)])]))
print("Exported", sum(map(len, domains.values())), "definitions;", len(hotacreatures), "HotA replacements")
