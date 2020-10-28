﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;
using UtilityScripts;
using Locations.Settlements;

public class DropRestrained : GoapAction {

    public DropRestrained() : base(INTERACTION_TYPE.DROP_RESTRAINED) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        actionIconString = GoapActionStateDB.Haul_Icon;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        logTags = new[] {LOG_TAG.Work};
    }

    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = "Carry Restrained", target = GOAP_EFFECT_TARGET.TARGET }, IsCarriedOrInInventory);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode actionNode) {
        base.Perform(actionNode);
        SetState("Drop Success", actionNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        OtherData[] otherData = node.otherData;
        if (otherData != null) {
            if (otherData.Length == 1 && otherData[0].obj is LocationStructure) {
                return otherData[0].obj as LocationStructure;
            } else if (otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile) {
                return otherData[0].obj as LocationStructure;
            }
        }
        return base.GetTargetStructure(node);
    }
    public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
        OtherData[] otherData = goapNode.otherData;
        if (otherData != null) {
            if (otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile) {
                return otherData[1].obj as LocationGridTile;
            }
        }
        return null;
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter);
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsDropTargetMissing(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName);
        //if (defaultTargetMissing == false) {
        //    //check the target's traits, if any of them can make this action invalid
        //    for (int i = 0; i < poiTarget.traitContainer.allTraits.Count; i++) {
        //        Trait trait = poiTarget.traitContainer.allTraits[i];
        //        if (trait.TryStopAction(goapType, actor, poiTarget, ref goapActionInvalidity)) {
        //            break; //a trait made this action invalid, stop loop
        //        }
        //    }
        //}
        return goapActionInvalidity;
    }
    private bool IsDropTargetMissing(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (poiTarget.IsAvailable() == false 
            || (poiTarget.gridTileLocation == null && node.actor.carryComponent.IsPOICarried(poiTarget) == false)) {
            return true;
        }
        return false;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (actor == poiTarget) {
                return false;
            }
            if (otherData != null) {
                if (otherData.Length == 1 && otherData[0].obj is LocationStructure structure) {
                    return actor.movementComponent.HasPathToEvenIfDiffRegion(CollectionUtilities.GetRandomElement(structure.passableTiles));
                } else if (otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile targetTile) {
                    return actor.movementComponent.HasPathToEvenIfDiffRegion(targetTile);
                }
            }
            return true;
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool IsCarriedOrInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        // if (poiTarget is Character) {
        //     Character target = poiTarget as Character;
        //     return target.currentParty == actor.currentParty;    
        // } else {
        //     return actor.ownParty.IsPOICarried(poiTarget);
        // }
        return actor.IsPOICarriedOrInInventory(poiTarget);
    }
    #endregion

    #region State Effects
    //public void PreDropSuccess(ActualGoapNode goapNode) {
    //    //GoapActionState currentState = this.states[goapNode.currentStateName];
    //    goapNode.descriptionLog.AddToFillers(goapNode.actor.currentStructure, goapNode.actor.currentStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    public void AfterDropSuccess(ActualGoapNode goapNode) {
        //Character target = goapNode.poiTarget as Character;
        OtherData[] otherData = goapNode.otherData;
        LocationGridTile tile = null;
        if (otherData != null) {
            if (otherData.Length == 2 && otherData[0].obj is LocationStructure && otherData[1].obj is LocationGridTile) {
                tile = otherData[1].obj as LocationGridTile;
            }
        }
        goapNode.actor.UncarryPOI(goapNode.poiTarget, dropLocation: tile);
        if(goapNode.poiTarget is Character targetCharacter) {
            BaseSettlement currentSettlement = goapNode.actor.currentSettlement;
            if (goapNode.associatedJobType == JOB_TYPE.APPREHEND && currentSettlement != null && currentSettlement is NPCSettlement settlement && targetCharacter.currentStructure == settlement.prison) {
                if (targetCharacter.traitContainer.HasTrait("Criminal")) {
                    Criminal criminalTrait = targetCharacter.traitContainer.GetTraitOrStatus<Criminal>("Criminal");
                    criminalTrait.SetIsImprisoned(true);
                }    
            } else if (goapNode.associatedJobType == JOB_TYPE.SNATCH) {
                //snatcher specific behaviour
                HexTile hexTileLocation = targetCharacter.hexTileLocation;
                if(hexTileLocation != null) {
                    LocationStructure structure = hexTileLocation.GetMostImportantStructureOnTile();
                    if (structure is DemonicStructure) {
                        if (structure is Kennel) {
                            List<LocationGridTile> choices = structure.passableTiles.Where(t => (t.objHere == null || t.IsPassable()) && !(t.objHere is DoorTileObject)).ToList();
                            if (choices.Count > 0) {
                                LocationGridTile randomTile = CollectionUtilities.GetRandomElement(choices);
                                targetCharacter.marker.PlaceMarkerAt(randomTile);
                            } else {
                                Debug.LogWarning($"{goapNode.actor.name} could not place {targetCharacter.name} in a room in kennel, because no valid tiles could be found.");
                            }
                        } else if (structure.rooms != null && structure.rooms.Length > 0) {
                            //place target in a random room
                            List<StructureRoom> roomChoices = structure.rooms.Where(r => r.CanUnseizeCharacterInRoom(targetCharacter)).ToList();
                            if (roomChoices.Count > 0) {
                                StructureRoom randomRoom = CollectionUtilities.GetRandomElement(roomChoices);
                                List<LocationGridTile> choices = randomRoom.tilesInRoom.Where(t => (t.objHere == null || t.IsPassable()) && !(t.objHere is DoorTileObject)).ToList();
                                if (choices.Count > 0) {
                                    LocationGridTile randomTileInRoom = CollectionUtilities.GetRandomElement(choices);
                                    targetCharacter.marker.PlaceMarkerAt(randomTileInRoom);
                                    DoorTileObject door = randomRoom.GetTileObjectInRoom<DoorTileObject>(); //close door in room
                                    door?.Close();
                                } else {
                                    Debug.LogWarning($"{goapNode.actor.name} could not place {targetCharacter.name} in a room, because no valid tiles in room could be found.");
                                }

                            } else {
                                Debug.LogWarning($"{goapNode.actor.name} could not place {targetCharacter.name} in a room, because no valid rooms could be found.");
                            }
                        }
                    }
                }
                goapNode.actor.behaviourComponent.SetIsSnatching(false);
            }
        }

        if (goapNode.associatedJobType == JOB_TYPE.KIDNAP_RAID) {
            if (goapNode.actor.partyComponent.hasParty && goapNode.actor.partyComponent.currentParty.isActive && goapNode.actor.partyComponent.currentParty.currentQuest is RaidPartyQuest) {
                goapNode.actor.partyComponent.currentParty.RemoveMemberThatJoinedQuest(goapNode.actor);
            }
        }
    }
    #endregion
}