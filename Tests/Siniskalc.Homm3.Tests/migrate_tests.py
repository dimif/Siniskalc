import pathlib
import json
import re

root = pathlib.Path("Tests") / "Siniskalc.Homm3.Tests"
data = pathlib.Path("Tests") / "Siniskalc.Homm3.Tests" / "CatalogData" / "Baseline"
domains = {
    "Faction": "Factions", "CreatureAbility": "Abilities", "Building": "Buildings",
    "Creature": "Creatures", "Spell": "Spells", "HeroClass": "HeroClasses",
    "SecondarySkill": "SecondarySkills", "WarMachine": "WarMachines", "Artifact": "Artifacts",
    "VisitedPlaceKind": "VisitedPlaces",
}
for file in root.rglob("*.cs"):
    if file.name in ["CatalogExport.cs", "TestCatalog.cs"]:
        continue
    content = file.read_text(encoding="utf-8-sig")
    old = content
    versions = set(re.findall(r"GameVersion\.(\w+)", content)) - {"Unknown"}
    version = next(iter(versions)) if len(versions) == 1 else "HornOfTheAbyss"
    if file.name.startswith("Roe"):
        version = "RestorationOfErathia"
    elif file.name.startswith("Sod"):
        version = "ShadowOfDeath"
    catalog = f"TestCatalog.For(GameVersion.{version})"
    content = content.replace("Entities.TestCatalog", "TestCatalog")
    for kind, plural in domains.items():
        ids = [r["id"] for r in json.loads((data / (kind + ".json")).read_text())]
        for id in sorted(ids, key=len, reverse=True):
            content = re.sub(rf"\b{kind}\.{id}\b", f'{catalog}.{plural}.ById("{id}")', content)
        if kind == "Artifact":
            content = content.replace("Artifact.All.Values", f"{catalog}.WearableArtifacts")
            content = content.replace("Artifact.All", f"{catalog}.WearableArtifacts.ToDictionary(a => a.Code)")
            content = content.replace("Artifact.CarriedInBackpack", f"{catalog}.BackpackArtifacts.ToDictionary(a => a.Code)")
            content = content.replace("Artifact.FindByName(", f"{catalog}.FindArtifact(")
            for name, slot in [("Helmets", "Helmet"), ("Necklaces", "Necklace"), ("Torsos", "Torso"), ("Capes", "Cape"), ("Feet", "Feet"), ("Weapons", "Weapon"), ("Shields", "Shield"), ("Rings", "Ring"), ("Misc", "Misc")]:
                content = content.replace(f"Artifact.{name}", f"{catalog}.WearableArtifacts.Where(a => a.Slot.HasFlag(ArtifactSlots.{slot})).ToList()")
            content = content.replace("Artifact.Combination", f"{catalog}.WearableArtifacts.Where(a => a.IsCombination).ToList()")
        else:
            content = content.replace(f"{kind}.All", f"{catalog}.{plural}.All")
        content = re.sub(rf"\b(?:AbstractEnumeration\.)?FromCode<{kind}>\(", f"{catalog}.{plural}.Find(", content)
        content = content.replace(f"{kind}.{catalog}", catalog)
        content = content.replace(f"{kind}.FromByte(", f"{catalog}.{plural}.Find(")
        content = content.replace(f"{kind}.FromName<{kind}>(", f"{catalog}.{plural}.FindByName(")
    content = content.replace("CreatureCodes.Instance", f"new CreatureCodes({catalog})")
    content = content.replace("SpellScroll.TryParse(", f"SpellScroll.TryParse({catalog}, ")
    content = re.sub(r"SpellScroll\.For\((GameVersion\.\w+)\)", r"SpellScroll.For(TestCatalog.For(\1))", content)
    # Constructors whose version argument has become an immutable catalog.
    for kind in ["ArmyViewModel", "SpellsViewModel", "SecondarySkillsViewModel", "ArtifactFinderViewModel"]:
        content = re.sub(rf"new {kind}\(\s*(GameVersion\.\w+)", rf"new {kind}(TestCatalog.For(\1)", content)
    for kind in ["CastleDeserializer", "DwellingDeserializer", "MerchantStockDeserializer", "MerchantBytesLoader", "DwellingBytesLoader", "SafeMerchantStockSerializer"]:
        content = content.replace(f"new {kind}()", f"new {kind}({catalog})")
    for kind in ["MineDeserializer", "SafeMineSerializer"]:
        content = content.replace(f"new {kind}()", f"new {kind}(new CreatureCodes({catalog}))")
    content = re.sub(r"new CastleBytesLoader\((Encoding\.\w+)\)", rf"new CastleBytesLoader(\1, {catalog})", content)
    content = re.sub(r'nameof\(TestCatalog\.For\(GameVersion\.\w+\)\.\w+\.ById\("(\w+)"\)\)', r'"\1"', content)
    for school in ["Water", "Earth", "Fire", "Wind", "All"]:
        content = content.replace("SpellSchool." + school, f'{catalog}.SpellSchools.ById("{school.lower()}")')
    content = re.sub(r"SpellGrantProvider\.OfLevel\((?!TestCatalog)", f"SpellGrantProvider.OfLevel({catalog}, ", content)
    for kind in ["CastleDeserializer", "DwellingDeserializer", "MerchantStockDeserializer", "MerchantBytesLoader", "DwellingBytesLoader", "SafeMerchantStockSerializer"]:
        content = re.sub(rf"({kind}\s+\w+\s*=\s*)new\(\)", rf"\1new({catalog})", content)
    for kind in ["MineDeserializer", "SafeMineSerializer"]:
        content = re.sub(rf"({kind}\s+\w+\s*=\s*)new\(\)", rf"\1new(new CreatureCodes({catalog}))", content)
    content = re.sub(r"(CastleBytesLoader\s+\w+\s*=\s*)new\((Encoding\.\w+)\)", rf"\1new(\2, {catalog})", content)
    content = content.replace("new VersionSpecificEntitiesProviderFactory(version => new GrantedSpellsResolver(version))", "new VersionSpecificEntitiesProviderFactory(TestCatalog.Catalogs)")
    content = content.replace("new VersionSpecificEntitiesProviderFactory(version => Mock.Of<IGrantedSpellsResolver>())", "new VersionSpecificEntitiesProviderFactory(TestCatalog.Catalogs)")
    content = content.replace('base("test", new EffectLineProvider())', f'base("test", new EffectLineProvider(), {catalog})')
    if file.name == "MapPlacesLoaderTests.cs":
        content = re.sub(r"\.Load\((bytes|Array.Empty<byte>\(\)|\[\])\)", rf".Load(\1, {catalog})", content)
    if file.name == "SavegameParserTests.cs":
        content = content.replace(".Load(It.IsAny<byte[]>())", ".Load(It.IsAny<byte[]>(), It.IsAny<EntityCatalog>())")
        content = content.replace(".Load(bytes)", ".Load(bytes, It.IsAny<EntityCatalog>())")
    content = content.replace("kind.HeroOffset", "VisitedPlaceOffsets.For(kind.Slot)")

    def arguments(text, start):
        depth, quoted, escaped = 1, False, False
        args, part = [], start
        i = start
        while i < len(text):
            ch = text[i]
            if quoted:
                if escaped: escaped = False
                elif ch == "\\": escaped = True
                elif ch == '"': quoted = False
            elif ch == '"': quoted = True
            elif ch in "([{": depth += 1
            elif ch in ")]}":
                depth -= 1
                if depth == 0:
                    args.append(text[part:i].strip())
                    return args, i
            elif ch == "," and depth == 1:
                args.append(text[part:i].strip())
                part = i + 1
            i += 1
        raise ValueError(text[start:start+100])

    for kind in ["BackpackViewModel", "EquippedArtifactsViewModel"]:
        matches = list(re.finditer(rf"(?:new {kind}|{kind}\s+\w+\([^;{{}}]*?\)\s*=>\s*new|{kind}\s+\w+\s*=\s*new)\(", content))
        for match in reversed(matches):
            args, end = arguments(content, match.end())
            if len(args) >= 3 and "TestCatalog" in args[2]:
                continue
            given = args[3] if len(args) >= 4 else None
            chosen = f"TestCatalog.For({given})" if given and given.startswith("GameVersion.") else catalog
            replacement = args[:2] + [chosen] + args[2:3] + args[4:]
            content = content[:match.end()] + ", ".join(replacement) + content[end:]
    if file.name == "MapPlacesLoaderTests.cs":
        for match in reversed(list(re.finditer(r"\.loader\.Load\(", content))):
            args, end = arguments(content, match.end())
            if len(args) == 1:
                content = content[:end] + ", TestCatalog.Hota" + content[end:]
    if file.name.startswith("Roe"):
        content = content.replace("TestCatalog.For(GameVersion.RestorationOfErathia)", "TestCatalog.For(GameVersion.ShadowOfDeath)")
    if file.name.startswith(("Roe", "SodSeries")):
        content = content.replace("TestServices.CastleDeserializer", "new CastleDeserializer(TestCatalog.For(GameVersion.ShadowOfDeath))")
    content = re.sub(r"TestServices\.CastleDeserializer\.Deserialize\(castles\[(.+?)\]\)", r"castles.GetModel(\1)", content)
    for index, name in [(0, "Ballista"), (1, "Ballista"), (2, "Cannon"), (3, "Ammo Cart"), (4, "First Aid Tent")]:
        content = content.replace(f'WarMachinesChecked[{index}]', f'WarMachineChoices.Single(c => c.Name == "{name}").IsChecked')
    content = re.sub(r'TestCatalog\.For\(GameVersion\.\w+\)\.Creatures\.ById\("(\w+)"\)\.GetInfo\(([^)]+)\)', r'TestCatalog.For(\2).Creatures.ById("\1").Info', content)
    content = re.sub(r'\bc\.GetInfo\(version\)', 'c.Info', content)
    content = content.replace("new MineBytesLoader()", "new MineBytesLoader(TestCatalog.Hota)")
    content = re.sub(r"(MineBytesLoader\s+\w+\s*=\s*)new\(\)", r"\1new(TestCatalog.Hota)", content)
    for match in reversed(list(re.finditer(r"new MineModel\(", content))):
        args, end = arguments(content, match.end())
        if len(args) == 5:
            content = content[:end] + ", TestCatalog.Hota" + content[end:]
    if content != old:
        file.write_text(content, encoding="utf-8-sig")
        print(file)
