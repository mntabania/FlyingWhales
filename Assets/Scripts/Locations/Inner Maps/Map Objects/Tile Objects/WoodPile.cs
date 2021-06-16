using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class WoodPile : ResourcePile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Wood;
    public WoodPile() : base(RESOURCE.WOOD) {
        Initialize(TILE_OBJECT_TYPE.WOOD_PILE, false);
        SetResourceInPile(100);
    }
    public WoodPile(SaveDataTileObject data) : base(data, RESOURCE.WOOD) { }
    public override string ToString() {
        return $"Wood Pile {id.ToString()}";
    }

    #region Overrides
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (gridTileLocation != null && gridTileLocation.structure.structureType == STRUCTURE_TYPE.LUMBERYARD) {
            AddAdvertisedAction(INTERACTION_TYPE.BUY_WOOD);
        } else {
            RemoveAdvertisedAction(INTERACTION_TYPE.BUY_WOOD);
        }
    }
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
        RemoveAdvertisedAction(INTERACTION_TYPE.BUY_WOOD);
    }
    #endregion

    #region Reactions
    public override void GeneralReactionToTileObject(Character actor, ref string debugLog) {
        base.GeneralReactionToTileObject(actor, ref debugLog);
        if (actor is Troll) {
            if (actor.homeStructure != null && gridTileLocation.structure != actor.homeStructure && !actor.jobQueue.HasJob(JOB_TYPE.DROP_ITEM)) {
                actor.jobComponent.CreateHoardItemJob(this, actor.homeStructure, true);
            }
        }
    }
    #endregion
}