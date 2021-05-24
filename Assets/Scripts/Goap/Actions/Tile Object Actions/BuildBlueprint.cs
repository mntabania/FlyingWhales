using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Logs;
using UnityEngine;

public class BuildBlueprint : GoapAction {

    private Precondition _stonePrecondition;
    private Precondition _woodPrecondition;
    private Precondition _metalPrecondition;


    public BuildBlueprint() : base(INTERACTION_TYPE.BUILD_BLUEPRINT) {
        actionIconString = GoapActionStateDB.Build_Icon;
        showNotification = true;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};

        _stonePrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
        _woodPrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
        _metalPrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Metal Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
    }

    #region Overrides
    // protected override void ConstructBasePreconditionsAndEffects() {
    //     AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasSupply);
    // }
    public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden) {
        if (target is GenericTileObject genericTileObject) {
            if (genericTileObject.blueprintOnTile != null) {
                //List<Precondition> baseP = base.GetPrecondition(actor, target, otherData, out isOverridden);
                //List<Precondition> p = ObjectPoolManager.Instance.CreateNewPreconditionsList();
                Precondition p = null;
                //p.AddRange(baseP);

                if (genericTileObject.blueprintOnTile.craftCost > 0) {
                    switch (genericTileObject.blueprintOnTile.thinWallResource) {
                        case RESOURCE.STONE:
                            p = _stonePrecondition;
                            break;
                        case RESOURCE.WOOD:
                            p = _woodPrecondition;
                            break;
                        case RESOURCE.METAL:
                            p = _metalPrecondition;
                            break;
                        default:
                            p = _woodPrecondition;
                            break;
                    }
                    isOverridden = true;
                    return p;    
                }
            }
        }
        return base.GetPrecondition(actor, target, otherData, jobType, out isOverridden);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Build Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode goapNode) {
        base.AddFillersToLog(log, goapNode);
        if (goapNode.poiTarget is GenericTileObject genericTileObject && genericTileObject.blueprintOnTile != null) {
            log.AddToFillers(null, genericTileObject.blueprintOnTile.structureType.StructureName(), LOG_IDENTIFIER.STRING_1);
        }
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        actor.UncarryPOI();
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.UncarryPOI();
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            if (poiTarget is GenericTileObject genericTileObject) {
                if (genericTileObject.blueprintOnTile == null) {
                    return false;
                }  
            } else {
                return false;
            }
            //TODO:
            // StructureTileObject structure = poiTarget as StructureTileObject;
            // return structure.spot.hasBlueprint;
            return actor.homeSettlement != null;
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasResource(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        if (poiTarget is GenericTileObject genericTileObject && genericTileObject.blueprintOnTile != null) {
            if (poiTarget.resourceStorageComponent.HasResourceAmount(genericTileObject.blueprintOnTile.thinWallResource, genericTileObject.blueprintOnTile.craftCost)) {
                return true;
            }
            //return actor.ownParty.isCarryingAnyPOI && actor.ownParty.carriedPOI is ResourcePile;
            if (actor.carryComponent.isCarryingAnyPOI && actor.carryComponent.carriedPOI is ResourcePile resourcePile) {
                return resourcePile.providedResource == genericTileObject.blueprintOnTile.thinWallResource && resourcePile.resourceInPile >= genericTileObject.blueprintOnTile.craftCost;
            }    
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreBuildSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is GenericTileObject genericTileObject) {
            if (goapNode.actor.carryComponent.carriedPOI is ResourcePile carriedPile) {
                carriedPile.AdjustResourceInPile(-genericTileObject.blueprintOnTile.craftCost);
                goapNode.poiTarget.resourceStorageComponent.AdjustResource(carriedPile.specificProvidedResource, genericTileObject.blueprintOnTile.craftCost);    
            }
            goapNode.descriptionLog.AddToFillers(null, genericTileObject.blueprintOnTile.structureType.StructureName(), LOG_IDENTIFIER.STRING_1);
        }
    }
    public void AfterBuildSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is GenericTileObject genericTileObject) {
            LocationGridTile connectorTile = (LocationGridTile)goapNode.otherData[0].obj;
            genericTileObject.BuildBlueprintOnTile(goapNode.actor.homeSettlement, connectorTile);

            if(genericTileObject.blueprintOnTile != null) {
                //After successfully building house, no house faction leader/settlement ruler/nobles should have first dibs on the newly built house
                if(genericTileObject.blueprintOnTile.structureType == STRUCTURE_TYPE.DWELLING && goapNode.actor.homeSettlement != null) {
                    Character importantCharacterThatShouldSetHome = null;
                    for (int i = 0; i < goapNode.actor.homeSettlement.residents.Count; i++) {
                        Character resident = goapNode.actor.homeSettlement.residents[i];
                        if(resident.isFactionLeader || resident.isSettlementRuler || resident.characterClass.className == "Noble") {
                            if(resident.homeStructure == null 
                                || (resident.homeStructure.structureType != STRUCTURE_TYPE.DWELLING && resident.homeStructure.structureType != STRUCTURE_TYPE.VAMPIRE_CASTLE)) {
                                importantCharacterThatShouldSetHome = resident;
                                break;
                            }
                        }
                    }
                    if(importantCharacterThatShouldSetHome != null) {
                        importantCharacterThatShouldSetHome.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                    }
                }
            }
        }
        //PlayerUI.Instance.ShowGeneralConfirmation("New Structure", $"A new {structure.name} has been built at {spot.gridTileLocation.structure.location.name}");
    }
    #endregion
}

