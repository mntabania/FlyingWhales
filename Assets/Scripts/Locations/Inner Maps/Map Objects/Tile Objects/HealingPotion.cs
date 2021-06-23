using System.Collections;
using System.Collections.Generic;
using Characters.Villager_Wants;
using Inner_Maps;
using UnityEngine;

public class HealingPotion : TileObject {

    public HealingPotion() {
        Initialize(TILE_OBJECT_TYPE.HEALING_POTION, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
        AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
    }
    public HealingPotion(SaveDataTileObject data) : base(data) { }

    #region Overrides
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (gridTileLocation != null && gridTileLocation.structure.structureType == STRUCTURE_TYPE.HOSPICE) {
            AddAdvertisedAction(INTERACTION_TYPE.BUY_ITEM);
        } else {
            RemoveAdvertisedAction(INTERACTION_TYPE.BUY_ITEM);
        }
    }
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
        RemoveAdvertisedAction(INTERACTION_TYPE.BUY_ITEM);
    }
    #endregion
    
    #region Reactions
    public override void VillagerReactionToTileObject(Character actor, ref string debugLog) {
        base.VillagerReactionToTileObject(actor, ref debugLog);
        if (actor.villagerWantsComponent != null && actor.villagerWantsComponent.IsWantToggledOn<HealingPotionWant>() && !actor.jobQueue.HasJob(JOB_TYPE.OBTAIN_WANTED_ITEM)) {
            bool shouldPickUp = false;
            if (structureLocation.structureType.IsVillageStructure() && 
                structureLocation.structureType != STRUCTURE_TYPE.CITY_CENTER && structureLocation.structureType != STRUCTURE_TYPE.CEMETERY) {
                shouldPickUp = IsOwnedBy(actor) && structureLocation != actor.homeStructure;
            } else {
                shouldPickUp = characterOwner == null || IsOwnedBy(actor);
            }
            if (shouldPickUp) {
                actor.jobComponent.CreateTakeItemJob(JOB_TYPE.OBTAIN_WANTED_ITEM, this);
            }
        }
    }
    #endregion
}
