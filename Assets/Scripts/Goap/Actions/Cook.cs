using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Traits;

public class Cook : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Cook() : base(INTERACTION_TYPE.COOK) {
        //actionLocationType = ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET }, IsCarried);
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Cook Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 1;
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter, addToLocation: false);
        if (targetCharacter != null && targetCharacter.hasMarker) {
            targetCharacter.marker.SetActiveState(true);    
        }
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter, addToLocation: false);
        if (targetCharacter != null && targetCharacter.hasMarker) {
            targetCharacter.marker.SetActiveState(true);    
        }
        
    }
    public override void OnInvalidAction(ActualGoapNode node) {
        base.OnInvalidAction(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter, addToLocation: false);
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        Character actor = node.actor;
        OtherData[] otherData = node.otherData;
        if (otherData != null && otherData.Length == 1 && otherData[0].obj is TileObject tileObject) {
            if(tileObject.gridTileLocation != null) {
                return tileObject.structureLocation;
            }
            return null;
        }
        return base.GetTargetStructure(node);
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        if (goapNode.otherData != null && goapNode.otherData.Length == 1 && goapNode.otherData[0].obj is TileObject) {
            return goapNode.otherData[0].obj as TileObject;
        }
        return base.GetTargetToGoTo(goapNode);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        string stateName = "Target Missing";
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(false, stateName);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (poiTarget is Character targetCharacter) {
                //if (!targetCharacter.isDead) {
                //    goapActionInvalidity.isInvalid = true;
                //    goapActionInvalidity.reason = "target_dead";
                //} else 
                if (!node.actor.carryComponent.IsPOICarried(targetCharacter)) {
                    goapActionInvalidity.isInvalid = true;
                    goapActionInvalidity.reason = "target_unavailable";
                }
            } else {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.reason = "target_unavailable";
            }
        }
        return goapActionInvalidity;
    }
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    //public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
    //    string reaction = base.ReactionToActor(actor, target, witness, node, status);
    //    if (!actor.isNormalCharacter && witness.homeSettlement != null && witness.faction != null && actor.homeStructure != null && target is Character targetCharacter) {
    //        Prisoner prisoner = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
    //        if (node.targetStructure == actor.homeStructure || (prisoner != null && prisoner.IsConsideredPrisonerOf(actor))) {
    //            string relationshipName = witness.relationshipContainer.GetRelationshipNameWith(targetCharacter);
    //            if (relationshipName == RelationshipManager.Acquaintance || witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
    //                witness.faction.partyQuestBoard.CreateExterminatePartyQuest(witness, witness.homeSettlement, actor.homeStructure, witness.homeSettlement);    
    //            }    
    //        }
    //    }
    //    return reaction;
    //}
    public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
        if (target is Character targetCharacter) {
            if (witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Despair);
                    reactions.Add(EMOTION.Sadness);    
                }
            } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Despair);
                    reactions.Add(EMOTION.Sadness);    
                }
            }
        }
    }
    #endregion

    #region State Effects
    public void PreCookSuccess(ActualGoapNode goapNode) {
        if(goapNode.poiTarget is Character targetCharacter) {
            goapNode.actor.UncarryPOI(addToLocation: false);
            // if (targetCharacter.currentRegion != null) {
            //     targetCharacter.currentRegion.RemoveCharacterFromLocation(targetCharacter);
            // }
            targetCharacter.marker.SetActiveState(false);
        }
    }
    public void AfterCookSuccess(ActualGoapNode goapNode) {
        if(goapNode.poiTarget is Character targetCharacter) {
            goapNode.actor.UncarryPOI(addToLocation: false);
            if (targetCharacter.isDead) {
                if (targetCharacter.hasMarker) {
                    targetCharacter.DestroyMarker();
                }
                if (targetCharacter.currentRegion != null) {
                    targetCharacter.currentRegion.RemoveCharacterFromLocation(targetCharacter);
                }
            } else {
                targetCharacter.SetDestroyMarkerOnDeath(true);
                targetCharacter.Death(deathFromAction: goapNode, responsibleCharacter: goapNode.actor, _deathLog: goapNode.descriptionLog);    
            }
        }
        CharacterManager.Instance.CreateFoodPileForPOI(goapNode.poiTarget, goapNode.actor.gridTileLocation);
    }
    #endregion

    #region Preconditions
    private bool IsCarried(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        // Character target = poiTarget as Character;
        // return target.currentParty == actor.currentParty;
        return actor.IsPOICarriedOrInInventory(poiTarget);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            Character targetCharacter = poiTarget as Character;
            //if (targetCharacter.marker == null) {
            //    return false;
            //}
            if (targetCharacter.isBeingCarriedBy != null && targetCharacter.isBeingCarriedBy != actor) {
                return false;
            }
            return true;
        }
        return false;
    }
    #endregion
}
