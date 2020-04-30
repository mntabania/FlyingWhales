using Inner_Maps;
using UtilityScripts;

public class Chicken : Animal {
    
    public override string raceClassName => "Chicken";

    public Chicken() : base(SUMMON_TYPE.Chicken, "Chicken", RACE.CHICKEN) {
        combatComponent.SetCombatMode(COMBAT_MODE.Passive);
    }
    public Chicken(string className) : base(SUMMON_TYPE.Chicken, className, RACE.CHICKEN) {
        combatComponent.SetCombatMode(COMBAT_MODE.Passive);
    }
    public Chicken(SaveDataCharacter data) : base(data) {
        combatComponent.SetCombatMode(COMBAT_MODE.Passive);
    }
    
    #region Listeners
    public override void SubscribeToSignals() {
        base.SubscribeToSignals();
        Messenger.AddListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    }
    public override void UnsubscribeSignals() {
        base.UnsubscribeSignals();
        Messenger.RemoveListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    }
    private void OnCharacterFinishedAction(ActualGoapNode goapNode) {
        if (goapNode.actor == this && goapNode.action.goapType == INTERACTION_TYPE.STAND &&
            gridTileLocation.collectionOwner.isPartOfParentRegionMap &&
            UnityEngine.Random.Range(0, 100) < 15 && gridTileLocation.HasUnoccupiedNeighbour
                (out var tiles, true)) {
            LocationGridTile randomTile = CollectionUtilities.GetRandomElement(tiles);
            if (gridTileLocation.structure.structureType == STRUCTURE_TYPE.CAVE) {
                randomTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.MUSHROOM), randomTile);
            } else if (gridTileLocation.structure.structureType.IsOpenSpace() 
                       && (gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.biomeType == BIOMES.GRASSLAND
                           || gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.biomeType == BIOMES.FOREST
                           || gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.biomeType == BIOMES.DESERT)) {
                randomTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HERB_PLANT), randomTile);
            }
        }
    }
    #endregion
}
