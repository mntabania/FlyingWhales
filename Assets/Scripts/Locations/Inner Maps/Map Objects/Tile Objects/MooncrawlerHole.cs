using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class MooncrawlerHole : AnimalBurrow {
    public MooncrawlerHole() : base(SUMMON_TYPE.Moonwalker){
        Initialize(TILE_OBJECT_TYPE.MOONCRAWLER_HOLE);
    }
    public MooncrawlerHole(SaveDataTileObject data) : base(data, SUMMON_TYPE.Moonwalker) { }
    
    #region Listeners
    protected override void SubscribeListeners() {
        base.SubscribeListeners();
        Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
    }
    protected override void UnsubscribeListeners() {
        base.UnsubscribeListeners();
        Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
    }
    private void OnHourStarted() {
        if (GameManager.Instance.currentTick == GameManager.Instance.GetTicksBasedOnHour(24)) {
            TrySpawnAnimalsOnMidnight();    
        }
    }
    #endregion
    
    private void TrySpawnAnimalsOnMidnight() {
        if (spawnedMonsters.Count <= 2) {
            List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
            Area area = gridTileLocation.area;
            for (int i = 0; i < area.gridTileComponent.passableTiles.Count; i++) {
                LocationGridTile tile = area.gridTileComponent.passableTiles[i];
                if (tile.structure is Wilderness) {
                    tiles.Add(tile);
                }
            }
            CreateNewMonster(tiles);
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