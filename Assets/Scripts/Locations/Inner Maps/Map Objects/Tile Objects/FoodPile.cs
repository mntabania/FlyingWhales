using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public abstract class FoodPile : ResourcePile {
    
    protected FoodPile(TILE_OBJECT_TYPE tileObjectType) : base(RESOURCE.FOOD) {
        Initialize(tileObjectType, false);
        //traitContainer.RemoveTrait(this, "Flammable");
        traitContainer.AddTrait(this, "Edible");
        SetResourceInPile(20);
    }
    protected FoodPile(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject, RESOURCE.FOOD) { }
    
    #region Overrides
    public override string ToString() {
        return $"Food Pile {id.ToString()}";
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (gridTileLocation != null && gridTileLocation.structure.structureType != STRUCTURE_TYPE.DWELLING
                                     && gridTileLocation.structure.structureType != STRUCTURE_TYPE.CITY_CENTER) {
            AddAdvertisedAction(INTERACTION_TYPE.BUY_FOOD);
        } else {
            RemoveAdvertisedAction(INTERACTION_TYPE.BUY_FOOD);
        }
    }
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
        RemoveAdvertisedAction(INTERACTION_TYPE.BUY_FOOD);
    }
    #endregion
    
    #region Eating
    public virtual void ApplyFoodEffectsToConsumer(Character p_consumer) { }
    #endregion

    #region Reactions
    public override void GeneralReactionToTileObject(Character actor, ref string debugLog) {
        base.GeneralReactionToTileObject(actor, ref debugLog);
        if (actor.homeSettlement != null) {
            //if character sees a resource pile that is outside his/her home settlement or
            //is not at his/her settlement's main storage
            //Do not haul to city center anymore because hauling is now a personal job and resource piles will be hauled to respective structures
            //if (resourcePile.gridTileLocation.IsPartOfSettlement(actor.homeSettlement) == false ||
            //    resourcePile.gridTileLocation.structure != actor.homeSettlement.mainStorage) {
            //    //do not create haul job for human and elven meat if actor is part of major faction
            //    if(actor.faction?.factionType.type == FACTION_TYPE.Ratmen) {
            //        if(resourcePile is FoodPile) {
            //            actor.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(resourcePile);
            //        }
            //    } else {
            //        bool cannotCreateHaulJob = (resourcePile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT || resourcePile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT) && actor.faction != null && actor.faction.isMajorNonPlayer;
            //        if (!cannotCreateHaulJob) {
            //            actor.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(resourcePile);
            //        }
            //    }
            //}
            if (actor.race.IsSapient()) {
                if ((tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT || tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT) &&
                    !actor.traitContainer.HasTrait("Cannibal") && !actor.traitContainer.HasTrait("Malnourished")) {
                    if (!actor.defaultCharacterTrait.HasAlreadyReactedToFoodPile(this)) {
                        actor.defaultCharacterTrait.AddFoodPileAsReactedTo(this);
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, this, $"saw {name}");
                    }
                    actor.jobComponent.TryCreateDisposeFoodPileJob(this);
                }
                bool isInterestedInFoodPile = false;
                if (actor.traitContainer.HasTrait("Cannibal")) {
                    isInterestedInFoodPile = true;
                } else {
                    isInterestedInFoodPile = tileObjectType != TILE_OBJECT_TYPE.HUMAN_MEAT && tileObjectType != TILE_OBJECT_TYPE.ELF_MEAT;
                }
                if (isInterestedInFoodPile) {
                    if (!actor.needsComponent.isStarving && !actor.partyComponent.isActiveMember && actor.homeStructure != null && 
                        gridTileLocation != null && gridTileLocation.structure != actor.homeStructure && gridTileLocation.structure.structureType != STRUCTURE_TYPE.DWELLING && 
                        !gridTileLocation.structure.structureType.IsFoodProducingStructure() && !actor.jobQueue.HasJob(JOB_TYPE.STOCKPILE_FOOD) && 
                        !actor.jobQueue.HasJob(JOB_TYPE.HAUL) && actor.movementComponent.HasPathToEvenIfDiffRegion(actor.homeStructure)) {
                        actor.jobComponent.CreateDropItemJob(JOB_TYPE.STOCKPILE_FOOD, this, actor.homeStructure);
                    }
                }
            }
        }
    }
    #endregion
}
