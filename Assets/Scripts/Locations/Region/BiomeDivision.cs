using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class BiomeDivision {
    public BIOMES biome { get; private set; }
    public List<LocationGridTile> tiles { get; }
    public List<Area> areas { get; }
    public MonsterMigrationBiomeAtomizedData[] faunaList { get; private set; }
    public int monsterMigrationChance { get; private set; }

    private WeightedDictionary<MonsterMigrationBiomeAtomizedData> _faunaListWeights;

    public BiomeDivision(BIOMES p_biome) {
        biome = p_biome;
        tiles = new List<LocationGridTile>();
        areas = new List<Area>();
        _faunaListWeights = new WeightedDictionary<MonsterMigrationBiomeAtomizedData>();
        Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
        AddListenersBasedOnBiome();
    }
    public BiomeDivision(SaveDataRegionDivision p_data) {
        biome = p_data.biome;
        faunaList = p_data.faunaList;
        monsterMigrationChance = p_data.monsterMigrationChance;
        tiles = new List<LocationGridTile>();
        areas = new List<Area>();
        _faunaListWeights = new WeightedDictionary<MonsterMigrationBiomeAtomizedData>();
        Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
        AddListenersBasedOnBiome();
    }
    public void AddTile(LocationGridTile p_tile) {
        tiles.Add(p_tile);
    }
    public void RemoveTile(LocationGridTile p_tile) {
        tiles.Remove(p_tile);
    }
    public void AddArea(Area p_area) {
        areas.Add(p_area);
    }
    public void RemoveArea(Area p_area) {
        areas.Remove(p_area);
    }

    #region Listeners
    private void OnDayStarted() {
        if (tiles.Count <= 0) { return; }
        if (!WorldSettings.Instance.worldSettingsData.mapSettings.disableAllMonsterMigrations && WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Affatt) {
            if(faunaList != null && faunaList.Length > 0) {
                MonsterMigrationPerDay();
            }
        }
    }
    #endregion

    #region Map Generation
    public void PopulateUnreservedFullyFlatTiles(List<Area> p_listToPopulate, List<Area> p_reservedAreas) {
        for (int i = 0; i < areas.Count; i++) {
            Area area = areas[i];
            if (area.elevationComponent.IsFully(ELEVATION.PLAIN) && !p_reservedAreas.Contains(area) && area.primaryStructureInArea is Wilderness) {
                p_listToPopulate.Add(area);
            }
        }
    }
    #endregion

    #region Monster Migration
    public void PopulateFaunaList(MonsterMigrationBiomeAtomizedData[] p_faunaList) {
        faunaList = p_faunaList;
    }
    public void PopulateFaunaList(int p_capacity) {
        faunaList = new MonsterMigrationBiomeAtomizedData[p_capacity];
        MonsterMigrationBiomeData migrationBiomeData = WorldSettings.Instance.GetMonsterMigrationBiomeDataByBiomeType(biome);
        if (migrationBiomeData != null) {
            _faunaListWeights.Clear();
            for (int i = 0; i < migrationBiomeData.dataList.Length; i++) {
                MonsterMigrationBiomeAtomizedData atomizedData = migrationBiomeData.dataList[i];
                if (atomizedData.weight > 0) {
                    _faunaListWeights.AddElement(atomizedData, atomizedData.weight);
                }
            }
            if (_faunaListWeights.Count > 0) {
                for (int i = 0; i < p_capacity; i++) {
                    if (_faunaListWeights.GetTotalOfWeights() <= 0) {
                        _faunaListWeights.Clear();
                        for (int j = 0; j < migrationBiomeData.dataList.Length; j++) {
                            MonsterMigrationBiomeAtomizedData atomizedData = migrationBiomeData.dataList[j];
                            if (atomizedData.weight > 0 && !faunaList.Contains(atomizedData)) {
                                _faunaListWeights.AddElement(atomizedData, atomizedData.weight);
                            }
                        }
                        if (_faunaListWeights.GetTotalOfWeights() <= 0) {
                            //could not find any more elements
                            break;
                        }
                    }
                    faunaList[i] = _faunaListWeights.PickRandomElementGivenWeights();
                    _faunaListWeights.RemoveElement(faunaList[i]);
                }
            }
            _faunaListWeights.Clear();
        }
    }
    private void MonsterMigrationPerDay() {
        TrySpawnMonstersInFaunaList();
    }
    private void TrySpawnMonstersInFaunaList() {
        if (GameUtilities.RollChance(monsterMigrationChance)) {
            LocationStructure homeStructureOfNewMonsters = null;
            Region region = tiles[0].parentMap.region;
            for (int i = 0; i < region.allStructures.Count; i++) {
                LocationStructure structure = region.allStructures[i];
                if (structure.structureType == STRUCTURE_TYPE.MONSTER_LAIR || structure.structureType == STRUCTURE_TYPE.CAVE) {
                    bool shouldSpawnMonsterAtStructure = true;
                    if (structure is Cave cave) {
                        shouldSpawnMonsterAtStructure = !cave.hasConnectedMine;
                    }
                    if (shouldSpawnMonsterAtStructure && HasTilePartOfThisBiomeDivision(structure)) {
                        if (!structure.IsOccupied()) {
                            homeStructureOfNewMonsters = structure;
                            break;
                        }
                    }
                }
            }

            if (homeStructureOfNewMonsters != null) {
                if(homeStructureOfNewMonsters.structureType == STRUCTURE_TYPE.CAVE
                    && (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom || WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa)
                    && CharacterManager.Instance.GenerateRatmen(homeStructureOfNewMonsters, GameUtilities.RandomBetweenTwoNumbers(1, 3), 8)) {
                    //Generate ratmen
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "WorldEvents", "Monster Migration", "monster_migration", providedTags: LOG_TAG.Major);
                    log.AddToFillers(null, "Ratmen", LOG_IDENTIFIER.STRING_1);
                    log.AddToFillers(homeStructureOfNewMonsters, homeStructureOfNewMonsters.name, LOG_IDENTIFIER.LANDMARK_1);
                    log.AddLogToDatabase();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                } else {
                    SpawnMonstersFaunaListProcessing(homeStructureOfNewMonsters);
                }
            }
        }
        //We did this so that on the first DAY_STARTED broadcast, there will be no spawning of monsters from the fauna list
        if (monsterMigrationChance == 0) {
            monsterMigrationChance = ChanceData.GetChance(CHANCE_TYPE.Monster_Migration);
        }
    }
    private bool HasTilePartOfThisBiomeDivision(LocationStructure p_structure) {
        for (int i = 0; i < p_structure.passableTiles.Count; i++) {
            LocationGridTile tile = p_structure.passableTiles[i];
            if (tile.mainBiomeType == biome) {
                return true;
            }
        }
        return false;
    }
    private void SpawnMonstersFaunaListProcessing(LocationStructure p_structure) {
        WeightedDictionary<MonsterMigrationBiomeAtomizedData> faunaWeights = TryGetFaunaListWeights();
        if (faunaWeights.Count > 0) {
            MonsterMigrationBiomeAtomizedData chosenData = faunaWeights.PickRandomElementGivenWeights();
            if (p_structure.structureType != STRUCTURE_TYPE.CAVE && chosenData.monsterType == SUMMON_TYPE.Fire_Elemental) {
                return; //temporarily disabled fire elemental spawning outside caves. Reference: https://trello.com/c/WfB4VaU8/4831-fire-elementals-usually-destroy-the-special-structure-that-they-live-at
            }
            int numOfSpawns = GameUtilities.RandomBetweenTwoNumbers(chosenData.minRange, chosenData.maxRange);
            for (int i = 0; i < numOfSpawns; i++) {
                LocationGridTile spawnLocationGridTile = p_structure.GetRandomPassableTile();
                Summon monster = CharacterManager.Instance.CreateNewSummon(chosenData.monsterType, FactionManager.Instance.GetDefaultFactionForMonster(chosenData.monsterType), homeLocation: p_structure.settlementLocation, homeRegion: p_structure.region, homeStructure: p_structure, bypassIdeologyChecking: true);
                CharacterManager.Instance.PlaceSummonInitially(monster, spawnLocationGridTile);
                if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pitto) {
                    monster.traitContainer.AddTrait(monster, "Mighty");
                }
            }
            string monsterTypeStr = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(chosenData.monsterType.ToString());
            monsterTypeStr = UtilityScripts.Utilities.PluralizeString(monsterTypeStr);
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "WorldEvents", "Monster Migration", "monster_migration", providedTags: LOG_TAG.Major);
            log.AddToFillers(null, monsterTypeStr, LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(p_structure, p_structure.name, LOG_IDENTIFIER.LANDMARK_1);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
        }
    }
    public MonsterMigrationBiomeAtomizedData GetRandomMonsterFromFaunaList() {
        WeightedDictionary<MonsterMigrationBiomeAtomizedData> faunaWeights = TryGetFaunaListWeights();
        return faunaWeights.PickRandomElementGivenWeights();
    }
    private WeightedDictionary<MonsterMigrationBiomeAtomizedData> TryGetFaunaListWeights() {
        if (_faunaListWeights.Count <= 0) {
            for (int i = 0; i < faunaList.Length; i++) {
                MonsterMigrationBiomeAtomizedData atomizedData = faunaList[i];
                if (atomizedData.weight > 0) {
                    _faunaListWeights.AddElement(atomizedData, atomizedData.weight);
                }
            }
        }
        return _faunaListWeights;
    }
    #endregion

    #region Testing
    public string GetTestingInfo() {
        string info = $"Tiles: {tiles.Count.ToString()}. Biome: {biome.ToString()}";
        info = $"{info}" + _faunaListWeights.GetWeightsSummary("\nFauna List:");
        return info;
    }
    #endregion
    
    #region Listeners
    private void AddListenersBasedOnBiome() {
        switch (biome) {
            case BIOMES.GRASSLAND:
                break;
            case BIOMES.SNOW:
            case BIOMES.TUNDRA:
                Messenger.AddListener(Signals.HOUR_STARTED, TryFreezeWetObjects);
                break;
            case BIOMES.DESERT:
                Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, TryRemoveFreezing);
                break;
            case BIOMES.FOREST:
                break;
        }
    }
    #endregion

    #region Snow
    private void TryFreezeWetObjects() {
        Messenger.Broadcast(AreaSignals.FREEZE_WET_OBJECTS);
    }
    #endregion
    
    #region Desert
    private void TryRemoveFreezing(Character character, Area p_area) {
        if (GameManager.Instance.gameHasStarted) {
            if (p_area.gridTileComponent.centerGridTile.mainBiomeType == biome) {
                character.traitContainer.RemoveTrait(character, "Freezing");
                character.traitContainer.RemoveTrait(character, "Frozen");
            }    
        }
    }
    #endregion
}

public class SaveDataRegionDivision : SaveData<BiomeDivision> {
    public BIOMES biome;
    public MonsterMigrationBiomeAtomizedData[] faunaList;

    public int monsterMigrationChance;

    #region Overrides
    public override void Save(BiomeDivision data) {
        biome = data.biome;
        faunaList = data.faunaList;
        monsterMigrationChance = data.monsterMigrationChance;
    }

    public override BiomeDivision Load() {
        BiomeDivision data = new BiomeDivision(this);
        return data;
    }
    #endregion
}