﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class RegionDivision {
    public BIOMES biome { get; private set; }
    public List<HexTile> tiles { get; }
    public MonsterMigrationBiomeAtomizedData[] faunaList { get; private set; }
    public int monsterMigrationChance { get; private set; }

    private WeightedDictionary<MonsterMigrationBiomeAtomizedData> _faunaListWeights;

    public RegionDivision(BIOMES p_biome) {
        biome = p_biome;
        tiles = new List<HexTile>();
        _faunaListWeights = new WeightedDictionary<MonsterMigrationBiomeAtomizedData>();
        Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
    }
    public RegionDivision(BIOMES p_biome, List<HexTile> p_tiles) {
        biome = p_biome;
        tiles = p_tiles;
        _faunaListWeights = new WeightedDictionary<MonsterMigrationBiomeAtomizedData>();
        Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
    }
    public RegionDivision(SaveDataRegionDivision p_data) {
        biome = p_data.biome;
        faunaList = p_data.faunaList;
        monsterMigrationChance = p_data.monsterMigrationChance;
        tiles = new List<HexTile>();
        _faunaListWeights = new WeightedDictionary<MonsterMigrationBiomeAtomizedData>();
        Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
    }
    public void AddTile(HexTile p_tile) {
        tiles.Add(p_tile);
        p_tile.SetRegionDivision(this);
    }

    #region Listeners
    private void OnDayStarted() {
        if (!WorldSettings.Instance.worldSettingsData.mapSettings.disableAllMonsterMigrations && WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Affatt) {
            if(faunaList != null && faunaList.Length > 0) {
                MonsterMigrationPerDay();
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
            Region region = tiles[0].region;
            for (int i = 0; i < region.allStructures.Count; i++) {
                LocationStructure structure = region.allStructures[i];
                if (structure.structureType == STRUCTURE_TYPE.MONSTER_LAIR || structure.structureType == STRUCTURE_TYPE.CAVE) {
                    if (structure.occupiedHexTile.regionDivision == this) {
                        if (!structure.IsOccupied()) {
                            homeStructureOfNewMonsters = structure;
                            break;
                        }
                    }
                }
            }

            if (homeStructureOfNewMonsters != null) {
                SpawnMonstersFaunaListProcessing(homeStructureOfNewMonsters);
            }
        }
        //We did this so that on the first DAY_STARTED broadcast, there will be no spawning of monsters from the fauna list
        if (monsterMigrationChance == 0) {
            monsterMigrationChance = 5;
        }
    }
    private void SpawnMonstersFaunaListProcessing(LocationStructure p_structure) {
        WeightedDictionary<MonsterMigrationBiomeAtomizedData> faunaWeights = TryGetFaunaListWeights();
        if (faunaWeights.Count > 0) {
            MonsterMigrationBiomeAtomizedData chosenData = faunaWeights.PickRandomElementGivenWeights();
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
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
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
}

public class SaveDataRegionDivision : SaveData<RegionDivision> {
    public BIOMES biome;
    public MonsterMigrationBiomeAtomizedData[] faunaList;

    public int monsterMigrationChance;

    #region Overrides
    public override void Save(RegionDivision data) {
        biome = data.biome;
        faunaList = data.faunaList;
        monsterMigrationChance = data.monsterMigrationChance;
    }

    public override RegionDivision Load() {
        RegionDivision data = new RegionDivision(this);
        return data;
    }
    #endregion
}