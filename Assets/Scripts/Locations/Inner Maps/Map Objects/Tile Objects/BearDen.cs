using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class BearDen : AnimalBurrow {
    
    private const int MaxBears = 4;
    
    public BearDen() : base(SUMMON_TYPE.Bear){
        Initialize(TILE_OBJECT_TYPE.BEAR_DEN);
    }
    public BearDen(SaveDataTileObject data) : base(data, SUMMON_TYPE.Bear) { }
    
    #region Listeners
    protected override void SubscribeListeners() {
        base.SubscribeListeners();
        Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
    }
    protected override void UnsubscribeListeners() {
        base.UnsubscribeListeners();
        Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
    }
    private void OnDayStarted() {
        TrySpawnAnimalsOnDayStarted();
    }
    #endregion
    
    private void TrySpawnAnimalsOnDayStarted() {
        if (GameUtilities.RollChance(35)) {
            if (spawnedMonsters.Count <= 0) {
                List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
                Area area = gridTileLocation.area;
                for (int i = 0; i < area.gridTileComponent.passableTiles.Count; i++) {
                    LocationGridTile tile = area.gridTileComponent.passableTiles[i];
                    if (tile.structure is Wilderness) {
                        tiles.Add(tile);
                    }
                }
                for (int i = 0; i < MaxBears; i++) {
                    CreateNewMonster(tiles);
                }
                RuinarchListPool<LocationGridTile>.Release(tiles);
            }    
        }
    }

    protected override void OnGameLoaded() {
        base.OnGameLoaded();
        List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
        Area area = gridTileLocation.area;
        for (int i = 0; i < area.gridTileComponent.passableTiles.Count; i++) {
            LocationGridTile tile = area.gridTileComponent.passableTiles[i];
            if (tile.structure is Wilderness) {
                tiles.Add(tile);
            }
        }
        for (int i = 0; i < MaxBears; i++) {
            CreateNewMonster(tiles);
        }
        RuinarchListPool<LocationGridTile>.Release(tiles);
    }
}