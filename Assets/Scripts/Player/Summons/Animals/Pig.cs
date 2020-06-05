using System;
using Inner_Maps;
using Traits;
using UtilityScripts;

public class Pig : Animal {
    public override string raceClassName => "Pig";
    public Pig() : base(SUMMON_TYPE.Pig, "Pig", RACE.PIG) {
        combatComponent.SetCombatMode(COMBAT_MODE.Passive);
    }
    public Pig(string className) : base(SUMMON_TYPE.Pig, className, RACE.PIG) {
        combatComponent.SetCombatMode(COMBAT_MODE.Passive);
    }
    public Pig(SaveDataCharacter data) : base(data) {
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
            (gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.biomeType == BIOMES.GRASSLAND 
             || gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.biomeType == BIOMES.FOREST 
             || gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.biomeType == BIOMES.DESERT) && 
            UnityEngine.Random.Range(0, 100) < 5 && gridTileLocation.structure.isInterior == false 
            && gridTileLocation.HasUnoccupiedNeighbour(out var tiles, true)) {
            LocationGridTile randomTile = CollectionUtilities.GetRandomElement(tiles);
            randomTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.TREE_OBJECT), randomTile);
        }
    }
    #endregion
}
