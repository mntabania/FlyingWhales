using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public abstract class AnimalBurrow : TileObject {
    public SUMMON_TYPE monsterToSpawn { get; private set; }
    public List<Summon> spawnedMonsters { get; private set; }

    private const int MaxMonsters = 5;

    #region getters
    public override Vector2 selectableSize => new Vector2(1.7f, 1.7f);
    public override Vector3 worldPosition {
        get {
            Vector2 pos = mapVisual.transform.position;
            pos.x += 0.5f;
            pos.y += 0.5f;
            return pos;
        }
    }
    public override Type serializedData => typeof(SaveDataAnimalBurrow);
    #endregion

    public AnimalBurrow(SUMMON_TYPE p_summonType) {
        monsterToSpawn = p_summonType;
        spawnedMonsters = new List<Summon>();
    }
    public AnimalBurrow(SaveDataTileObject data, SUMMON_TYPE p_summonType) : base(data) {
        monsterToSpawn = p_summonType;
    }

    #region Loading
    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        SaveDataAnimalBurrow saveDataAnimalBurrow = data as SaveDataAnimalBurrow;
        spawnedMonsters = SaveUtilities.ConvertIDListToMonsters(saveDataAnimalBurrow.spawnedMonsters);
    }
    #endregion
    
    protected override void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true) {
        base.Initialize(tileObjectType, shouldAddCommonAdvertisements);
        traitContainer.AddTrait(this, "Indestructible");
        traitContainer.AddTrait(this, "Immovable");
        RemovePlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT);
        RemovePlayerAction(PLAYER_SKILL_TYPE.POISON);
        RemovePlayerAction(PLAYER_SKILL_TYPE.IGNITE);
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
    }

    #region Listeners
    protected override void SubscribeListeners() {
        base.SubscribeListeners();
        Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
    }
    protected override void UnsubscribeListeners() {
        base.UnsubscribeListeners();
        Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
    }
    private void OnDayStarted() {
        if (spawnedMonsters.Count < MaxMonsters) {
            CreateNewMonster();
        }
    }
    private void OnCharacterDied(Character p_character) {
        if (p_character is Summon summon && spawnedMonsters.Contains(summon)) {
            spawnedMonsters.Remove(summon);
        }
    }
    private void OnGameLoaded() {
        List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
        Area area = gridTileLocation.area;
        for (int i = 0; i < area.gridTileComponent.passableTiles.Count; i++) {
            LocationGridTile tile = area.gridTileComponent.passableTiles[i];
            if (tile.structure is Wilderness) {
                tiles.Add(tile);
            }
        }
        for (int i = 0; i < MaxMonsters; i++) {
            CreateNewMonster(tiles);
        }
        RuinarchListPool<LocationGridTile>.Release(tiles);
    }
    #endregion

    private void CreateNewMonster(List<LocationGridTile> p_locationChoices = null) {
        Summon summon = CharacterManager.Instance.CreateNewSummon(monsterToSpawn, FactionManager.Instance.GetDefaultFactionForMonster(monsterToSpawn), null, gridTileLocation.parentMap.region);
        Area area = gridTileLocation.area;
        List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
        if (p_locationChoices != null) {
            tiles.AddRange(p_locationChoices);
        } else {
            for (int i = 0; i < area.gridTileComponent.passableTiles.Count; i++) {
                LocationGridTile tile = area.gridTileComponent.passableTiles[i];
                if (tile.structure is Wilderness) {
                    tiles.Add(tile);
                }
            }
        }
        if (tiles.Count > 0) {
            var tileLocation = CollectionUtilities.GetRandomElement(tiles);
            CharacterManager.Instance.PlaceSummonInitially(summon, tileLocation);
            summon.SetTerritory(area, false);
            spawnedMonsters.Add(summon);
#if DEBUG_LOG
            Debug.Log($"Placed {summon.name} on {tileLocation}. Passable tiles of area are: {area.gridTileComponent.passableTiles.Count.ToString()}.");
#endif
            if (p_locationChoices != null) { p_locationChoices.Remove(tileLocation); }
        }
        RuinarchListPool<LocationGridTile>.Release(tiles);
    }

    #region Testing
    public override string GetAdditionalTestingData() {
        string data = base.GetAdditionalTestingData();
        data = $"{data}\n\tSpawned Monsters: {spawnedMonsters.ComafyList()}";
        return data;
    }
    #endregion
}

#region Save Data
public class SaveDataAnimalBurrow : SaveDataTileObject {
    public List<string> spawnedMonsters;
    public override void Save(TileObject data) {
        base.Save(data);
        AnimalBurrow animalBurrow = data as AnimalBurrow;
        spawnedMonsters = SaveUtilities.ConvertSavableListToIDs(animalBurrow.spawnedMonsters);
    }
}
#endregion
