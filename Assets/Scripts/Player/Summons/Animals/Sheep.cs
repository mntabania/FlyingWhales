using Inner_Maps;
using UtilityScripts;

public class Sheep : ShearableAnimal {
    public override string raceClassName => "Sheep";
    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;
    public Sheep() : base(SUMMON_TYPE.Sheep, "Sheep", RACE.SHEEP) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Passive);
    }
    public Sheep(string className) : base(SUMMON_TYPE.Sheep, className, RACE.SHEEP) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Passive);
    }
    public Sheep(SaveDataShearableAnimal data) : base(data) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Passive);
    }

    public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.WOOL;

    // #region Listeners
    // public override void SubscribeToSignals() {
    //     base.SubscribeToSignals();
    //     Messenger.AddListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    // }
    // public override void UnsubscribeSignals() {
    //     base.UnsubscribeSignals();
    //     Messenger.RemoveListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    // }
    // private void OnCharacterFinishedAction(ActualGoapNode goapNode) {
    //     if (goapNode.actor == this && goapNode.action.goapType == INTERACTION_TYPE.STAND &&
    //         gridTileLocation.collectionOwner.isPartOfParentRegionMap && 
    //         (gridTileLocation.hexTileOwner.biomeType == BIOMES.GRASSLAND 
    //          || gridTileLocation.hexTileOwner.biomeType == BIOMES.FOREST 
    //          || gridTileLocation.hexTileOwner.biomeType == BIOMES.DESERT) && 
    //         UnityEngine.Random.Range(0, 100) < 5 && gridTileLocation.structure.isInterior == false 
    //         && gridTileLocation.HasUnoccupiedNeighbour(out var tiles, true)) {
    //         LocationGridTile randomTile = CollectionUtilities.GetRandomElement(tiles);
    //         randomTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.BERRY_SHRUB), randomTile);
    //     }
    // }
    // #endregion
}
