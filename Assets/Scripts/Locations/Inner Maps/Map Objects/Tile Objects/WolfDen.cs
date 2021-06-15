using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class WolfDen : AnimalBurrow {
    
    private const int MaxWolves = 4;
    public WolfDen() : base(SUMMON_TYPE.Wolf){
        Initialize(TILE_OBJECT_TYPE.WOLF_DEN);
    }
    public WolfDen(SaveDataTileObject data) : base(data, SUMMON_TYPE.Wolf) { }
    
    
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
                for (int i = 0; i < MaxWolves; i++) {
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
        for (int i = 0; i < MaxWolves; i++) {
            CreateNewMonster(tiles);
        }
        RuinarchListPool<LocationGridTile>.Release(tiles);
    }
}