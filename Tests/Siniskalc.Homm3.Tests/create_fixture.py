import copy
import json
import pathlib

root = pathlib.Path("Siniskalc.Homm3") / "GameData"
fixture = pathlib.Path("Tests") / "Siniskalc.Homm3.Tests" / "CatalogData" / "Synthetic"
fixture.mkdir(parents=True, exist_ok=True)

def save(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

def read(domain):
    return json.loads((root / "standard" / (domain + ".json")).read_text(encoding="utf-8"))

data = {p.stem: [] for p in (root / "standard").glob("*.json")}
def add(domain, source, id, code, **values):
    original = copy.deepcopy(next(d for d in read(domain) if d["id"] == source))
    original.update(id=id, code=code, order=max(d["order"] for d in read(domain)) + len(data[domain]) + 1,
                    name=id.replace("Fixture", "Astral "), availableFrom="hornOfTheAbyss", **values)
    data[domain].append(original)
    return original

add("factions", "Castle", "FixtureTown", 12, companionBits="fort")
add("abilities", "Flying", "FixtureBeam", 200, isRanged=True)
add("buildings", "CastleGuardhouse", "FixtureNest", 30, faction="FixtureTown", requires=[], upgrades=None)
add("buildings", "CastleUpgGuardhouse", "FixtureHighNest", 37, faction="FixtureTown", requires=[], upgrades="FixtureNest")
add("buildings", "CastleGriffinBastion", "FixtureHorde", 18, faction="FixtureTown", requires=["FixtureNest"], upgrades=None)
add("buildings", "CastleUpgGriffinBastion", "FixtureHighHorde", 19, faction="FixtureTown",
    requires=["FixtureHighNest"], upgrades="FixtureHorde", automatic=True)
scout = add("creatures", "Pikeman", "FixtureScout", 201, faction="FixtureTown", dwelling="FixtureNest")
scout["info"].update(health=19, growth=9, shots=12, special=["FixtureBeam"])
guard = add("creatures", "Halberdier", "FixtureGuard", 202, faction="FixtureTown", dwelling="FixtureHighNest", baseCreature="FixtureScout")
guard["info"].update(health=27, growth=9, shots=12, special=["FixtureBeam"])
add("heroClasses", "Knight", "FixtureChampion", 24, faction="FixtureTown", fallback=False)
add("heroClasses", "Cleric", "FixtureOracle", 25, faction="FixtureTown", fallback=False)
charm = add("artifacts", "HelmOfTheAlabasterUnicorn", "FixtureCharm", 200, cost=1500, role="none")
charm["effectInfo"].update(atk=2, other="A light from a JSON-only faction.",
    spellGrants=[dict(kind="named", spells=["Haste"], school=None, level=None, description="Haste")])
add("spellSchools", "wind", "fixtureSky", 5, description="The astral sky", universal=False)
haste = copy.deepcopy(next(d for d in read("spells") if d["id"] == "Haste"))
haste.update(replaces=True, school="fixtureSky")
data["spells"].append(haste)
add("warMachines", "Ballista", "FixtureLauncher", 200)
for domain, entries in data.items():
    save(fixture / (domain + ".json"), entries)
manifest = json.loads((root / "manifest.json").read_text())
manifest["sets"]["synthetic"] = dict(parent="hota", directory="synthetic")
manifest["versions"]["hornOfTheAbyss"] = "synthetic"
save(fixture / "manifest.json", manifest)

# Schemas assist editors; runtime validation remains authoritative for references and binary semantics.
nullable_strings = {"faction", "upgrades", "baseCreature", "dwelling", "combinesInto", "school", "parent"}
enum_properties = {"availableFrom", "companionBits", "type", "role", "class", "encoding", "slot"}
def schema(values, property_name=""):
    nonnull = [v for v in values if v is not None]
    if not nonnull:
        return {"type": ["string", "null"] if property_name in nullable_strings else ["integer", "null"]}
    sample = nonnull[0]
    if isinstance(sample, dict):
        names = list(dict.fromkeys(k for v in nonnull for k in v))
        properties = {k: schema([v[k] for v in nonnull if k in v], k) for k in names}
        required = [k for k in names if k != "replaces"]
        result = dict(type="object", properties=properties, required=required, additionalProperties=False)
    elif isinstance(sample, list):
        flat = [item for v in nonnull for item in v]
        result = dict(type="array", items=schema(flat) if flat else {"type": "string"})
    elif isinstance(sample, bool):
        result = {"type": "boolean"}
    elif isinstance(sample, int):
        result = {"type": "integer"}
        if property_name == "code": result.update(minimum=0, maximum=255)
        if property_name == "order": result.update(minimum=0)
    else:
        result = {"type": "string"}
        if property_name in enum_properties and property_name != "slot":
            result["enum"] = sorted(set(nonnull))
        if property_name == "availableFrom":
            result["enum"] = ["restorationOfErathia", "armageddonsBlade", "shadowOfDeath", "heroesChronicles", "hornOfTheAbyss"]
    if len(nonnull) != len(values) or property_name in nullable_strings:
        result["type"] = [result["type"], "null"]
    return result
for p in (root / "standard").glob("*.json"):
    items = read(p.stem)
    definition = schema(items)
    definition["properties"]["replaces"] = {"type": "boolean"}
    value = {"$schema": "https://json-schema.org/draft/2020-12/schema",
             "title": "HoMM3 " + p.stem, "type": "array", "items": definition}
    save(root / "schemas" / (p.stem + ".schema.json"), value)
metadata = schema([json.loads((root / "mapObjects.json").read_text())])
metadata.update({"$schema": "https://json-schema.org/draft/2020-12/schema", "title": "HoMM3 supported map-object metadata"})
save(root / "schemas" / "mapObjects.schema.json", metadata)
version_names = list(manifest["versions"])
manifest_schema = {
    "$schema": "https://json-schema.org/draft/2020-12/schema",
    "title": "HoMM3 standard version manifest",
    "type": "object", "additionalProperties": False,
    "required": ["schemaVersion", "versions", "sets"],
    "properties": {
        "schemaVersion": {"const": 1},
        "versions": {"type": "object", "required": version_names, "additionalProperties": False,
                     "properties": {name: {"type": "string", "minLength": 1} for name in version_names}},
        "sets": {"type": "object", "minProperties": 1, "additionalProperties": {
            "type": "object", "additionalProperties": False, "required": ["parent", "directory"],
            "properties": {"parent": {"type": ["string", "null"]}, "directory": {"type": "string", "minLength": 1}}
        }}
    }
}
save(root / "schemas" / "manifest.schema.json", manifest_schema)
