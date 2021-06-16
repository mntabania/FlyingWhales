using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class StonePile : ResourcePile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Stone;
    public StonePile() : base(RESOURCE.STONE) {
        Initialize(TILE_OBJECT_TYPE.STONE_PILE, false);
        SetResourceInPile(100);
    }
    public StonePile(SaveDataTileObject data) : base(data, RESOURCE.STONE) { }
    
    public override string ToString() {
        return $"Stone Pile {id.ToString()}";
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (gridTileLocation != null && gridTileLocation.structure.structureType == STRUCTURE_TYPE.MINE) {
            AddAdvertisedAction(INTERACTION_TYPE.BUY_STONE);
        } else {
            RemoveAdvertisedAction(INTERACTION_TYPE.BUY_STONE);
        }
    }
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
        RemoveAdvertisedAction(INTERACTION_TYPE.BUY_STONE);
    }


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