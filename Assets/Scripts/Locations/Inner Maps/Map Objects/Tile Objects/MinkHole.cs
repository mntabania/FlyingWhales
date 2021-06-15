using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class MinkHole : AnimalBurrow {
    public MinkHole() : base(SUMMON_TYPE.Mink){
        Initialize(TILE_OBJECT_TYPE.MINK_HOLE);
    }
    public MinkHole(SaveDataTileObject data) : base(data, SUMMON_TYPE.Mink) { }
    
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
        if (spawnedMonsters.Count <= 2) {
            List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
            Area area = gridTileLocation.area;
            for (int i = 0; i < area.gridTileComponent.passableTiles.Count; i++) {
                LocationGridTile tile = area.gridTileComponent.passableTiles[i];
                if (tile.structure is Wilderness) {
                    tiles.Add(tile);
                }
            }
            for (int i = 0; i < 2; i++) {
                CreateNewMonster(tiles);
            }
            RuinarchListPool<LocationGridTile>.Release(tiles);
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
        for (int i = 0; i < 3; i++) {
            CreateNewMonster(tiles);
        }
        RuinarchListPool<LocationGridTile>.Release(tiles);
    }
}