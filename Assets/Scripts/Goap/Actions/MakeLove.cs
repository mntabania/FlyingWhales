using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using UnityEngine.Assertions;

public class MakeLove : GoapAction {

    public MakeLove() : base(INTERACTION_TYPE.MAKE_LOVE) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Flirt_Icon;
        // validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.EARLY_NIGHT, TIME_IN_WORDS.LATE_NIGHT, TIME_IN_WORDS.AFTER_MIDNIGHT, };
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.LESSER_DEMON, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs, LOG_TAG.Social};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.INVITED, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), IsTargetInvited);
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Make Love Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
#endif
        if (actor.traitContainer.HasTrait("Enslaved")) {
            if (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)) {
#if DEBUG_LOG
                costLog += $" +2000(Slave, target is not in actor's home)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return 2000;
            }
        }
        if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive) {
            if (actor.partyComponent.isActiveMember) {
#if DEBUG_LOG
                costLog += $" +2000(Active Party, Cannot make love)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return 2000;
            }
        }
        Character targetCharacter = target as Character;
        if (targetCharacter != null && targetCharacter.partyComponent.hasParty && targetCharacter.partyComponent.currentParty.isActive) {
            if (targetCharacter.partyComponent.isActiveMember) {
#if DEBUG_LOG
                costLog += $" +2000(Target is in Active Party, Cannot make love)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return 2000;
            }
        }
        int cost = UtilityScripts.Utilities.Rng.Next(90, 131);
#if DEBUG_LOG
        costLog += $" +{cost}(Initial)";
#endif
        if (job.jobType != JOB_TYPE.TRIGGER_FLAW) {
            TIME_IN_WORDS timeOfDay = GameManager.Instance.GetCurrentTimeInWordsOfTick();
            if (actor.race.IsSapient() && timeOfDay != TIME_IN_WORDS.EARLY_NIGHT && timeOfDay != TIME_IN_WORDS.LATE_NIGHT && timeOfDay != TIME_IN_WORDS.AFTER_MIDNIGHT) {
                cost += 2000;
#if DEBUG_LOG
                costLog += " +2000(Actor is sapient and Time is not Early Night/Late Night/After Midnight)";
#endif
            }    
        }
        Angry angry = actor.traitContainer.GetTraitOrStatus<Angry>("Angry");
        if (actor.traitContainer.HasTrait("Chaste") || (angry != null && angry.IsResponsibleForTrait(targetCharacter))) {
            cost += 2000;
#if DEBUG_LOG
            costLog += " +2000(Chaste or Angry at target)";
#endif
        }
        if (actor.traitContainer.HasTrait("Lustful")) {
            cost -= 40;
#if DEBUG_LOG
            costLog += " -40(Lustful)";
#endif
        } else {
            int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
            if (numOfTimesActionDone > 5) {
                cost += 2000;
#if DEBUG_LOG
                costLog += " +2000(Times Made Love > 5)";
#endif
            } else {
                int timesCost = 10 * numOfTimesActionDone;
                cost += timesCost;
#if DEBUG_LOG
                costLog += $" +{timesCost}(10 x Times Made Love)";
#endif
            }
        }
#if DEBUG_LOG
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter);
        //actor.needsComponent.AdjustDoNotGetBored(-1);
        //targetCharacter.needsComponent.AdjustDoNotGetBored(-1);

        List<TileObject> tileObjects = actor.gridTileLocation.structure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED);
        if(tileObjects != null && tileObjects.Count > 0) {
            Bed bed = tileObjects[0] as Bed;
            bed?.OnDoneActionToObject(actor.currentActionNode);
        }


        //targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Wooed");
        if (targetCharacter.currentActionNode != null && targetCharacter.currentActionNode.action == this) {
            targetCharacter.SetCurrentActionNode(null, null, null);
        }
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter);
        //actor.needsComponent.AdjustDoNotGetBored(-1);
        //targetCharacter.needsComponent.AdjustDoNotGetBored(-1);

        //targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Wooed");
        if (targetCharacter.currentActionNode != null && targetCharacter.currentActionNode.action == this) {
            targetCharacter.SetCurrentActionNode(null, null, null);
        }
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        Assert.IsNotNull(targetCharacter, $"Make love of {goapNode.actor.name} is not a character! {goapNode.poiTarget?.ToString() ?? "Null"}");
        return GetValidBedForActor(goapNode.actor, targetCharacter);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        if (goapActionInvalidity.isInvalid == false) {
            Character targetCharacter = node.poiTarget as Character;
            Assert.IsNotNull(targetCharacter, $"Make love of {node.actor.name} is not a character! {node.poiTarget?.ToString() ?? "Null"}");
            Bed targetBed = node.actor.gridTileLocation.tileObjectComponent.objHere as Bed;
            if (targetBed == null) {
                //check neighbours
                for (int i = 0; i < node.actor.gridTileLocation.neighbourList.Count; i++) {
                    LocationGridTile neighbour = node.actor.gridTileLocation.neighbourList[i];
                    if (neighbour.tileObjectComponent.objHere is Bed bed) {
                        targetBed = bed;
                    }
                }
            }
            if (targetBed == null ||  targetBed.IsAvailable() == false || targetBed.GetActiveUserCount() > 0) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Make Love Fail";
            }
        }
        return goapActionInvalidity;
    }
    public override void OnInvalidAction(ActualGoapNode node) {
        base.OnInvalidAction(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter);

        //targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Wooed");
        if (targetCharacter.currentActionNode != null && targetCharacter.currentActionNode.action == this) {
            targetCharacter.SetCurrentActionNode(null, null, null);
        }
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);

        if (status == REACTION_STATUS.WITNESSED) {
            //If witnessed
            reactions.Add(EMOTION.Shock);
        }

        if (target is Character targetCharacter) {
            if (actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER) == false) { //if actor and target are not lovers
                Character actorLover = CharacterManager.Instance.GetCharacterByID(actor.relationshipContainer
                    .GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER));
                if (actorLover != null) {
                    if (actorLover != targetCharacter) {
                        //if actor has a lover that is different from target
                        //actor considered Infraction.
                        //CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, 
                        //    CRIME_SEVERITY.Infraction);    
                        //CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, node.crimeType, node, status);
                    } else if (actorLover == witness) {
                        //if witness is lover of actor
                        reactions.Add(EMOTION.Betrayal);
                        reactions.Add(EMOTION.Disapproval);
                    }

                }

                Character targetLover = CharacterManager.Instance.GetCharacterByID(targetCharacter.relationshipContainer
                    .GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER));
                if (targetLover != null) {
                    if (witness == targetLover) {
                        //witness is lover of target
                        reactions.Add(EMOTION.Anger);
                    }
                    if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.IsFamilyMember(actor)) {
                        //if actor is friend/close friend or relative
                        reactions.Add(EMOTION.Betrayal);
                    } else {
                        reactions.Add(EMOTION.Resentment);
                    }
                }
            } else {
                //actor and target are lovers
                if (witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.AFFAIR)) {
                    //if witness and target have an affair
                    reactions.Add(EMOTION.Resentment);
                }
            }
        }
    }
    public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);

        if (status == REACTION_STATUS.WITNESSED) {
            //If witnessed
            reactions.Add(EMOTION.Shock);
        }

        if (target is Character targetCharacter) {
            if (actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER) == false) { //if actor and target are not lovers
                Character targetLover = CharacterManager.Instance.GetCharacterByID(targetCharacter.relationshipContainer
                    .GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER));
                if (targetLover != null) {
                    if (targetLover != actor) {
                        //if target has a lover that is different from actor
                        //target considered Infraction.
                        //CrimeManager.Instance.ReactToCrime(witness, targetCharacter, node, node.associatedJobType, 
                        //    CRIME_SEVERITY.Infraction);
                        //CrimeManager.Instance.ReactToCrime(witness, targetCharacter, actor, actor.factionOwner, node.crimeType, node, status);
                    } else if (targetLover == witness) {
                        //if witness is lover of target
                        reactions.Add(EMOTION.Betrayal);
                        reactions.Add(EMOTION.Disapproval);
                    }

                }

                Character actorLover = CharacterManager.Instance.GetCharacterByID(actor.relationshipContainer
                    .GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER));
                if (actorLover != null) {
                    if (witness == targetLover) {
                        //witness is lover of actor
                        reactions.Add(EMOTION.Anger);
                    }
                    if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.IsFamilyMember(actor)) {
                        //if actor is friend/close friend or relative
                        reactions.Add(EMOTION.Betrayal);
                    } else {
                        reactions.Add(EMOTION.Resentment);
                    }
                }
            } else {
                //actor and target are lovers
                if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR)) {
                    //if witness and actor have an affair
                    reactions.Add(EMOTION.Resentment);
                }
            }

        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (node.poiTarget is Character character) {
            if (node.actor.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.LOVER) == false) {
                return REACTABLE_EFFECT.Negative;
            }
        }
        return REACTABLE_EFFECT.Neutral;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        if(target is Character targetCharacter) {
            if ((actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER) == false && actor.relationshipContainer.HasRelationship(RELATIONSHIP_TYPE.LOVER))
                || (targetCharacter.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER) == false && targetCharacter.relationshipContainer.HasRelationship(RELATIONSHIP_TYPE.LOVER))){
                return CRIME_TYPE.Infidelity;
            }
        }
        return base.GetCrimeType(actor, target, crime);
    }
    public override bool IsHappinessRecoveryAction() {
        return true;
    }
#endregion

#region Effects
    public void PreMakeLoveSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        Character targetCharacter = goapNode.poiTarget as Character;
        Assert.IsNotNull(targetCharacter, $"Target character of Make Love Action by {goapNode.actor.name} is not a character!");
        Bed bed = null;
        if (actor.tileObjectComponent.primaryBed != null) {
            if(actor.tileObjectComponent.primaryBed.gridTileLocation != null 
                && (actor.gridTileLocation == actor.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(actor.tileObjectComponent.primaryBed.gridTileLocation, true))) {
                bed = actor.tileObjectComponent.primaryBed;
            }
        } else if (targetCharacter.tileObjectComponent.primaryBed != null) {
            if (targetCharacter.tileObjectComponent.primaryBed.gridTileLocation != null
                && (actor.gridTileLocation == targetCharacter.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(targetCharacter.tileObjectComponent.primaryBed.gridTileLocation, true))) {
                bed = targetCharacter.tileObjectComponent.primaryBed;
            }
        }
        Assert.IsNotNull(bed, $"Target bed of Make Love Action by {goapNode.actor.name} targeting {goapNode.poiTarget.name} is null!");

        goapNode.actor.UncarryPOI(targetCharacter, dropLocation: bed.gridTileLocation);

        bed.OnDoActionToObject(goapNode);

        //goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        //targetCharacter.needsComponent.AdjustDoNotGetBored(1);

        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
        targetCharacter.jobComponent.IncreaseNumOfTimesActionDone(this);

        targetCharacter.SetCurrentActionNode(goapNode.actor.currentActionNode, goapNode.actor.currentJob, goapNode.actor.currentPlan);
        goapNode.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void PerTickMakeLoveSuccess(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        goapNode.actor.needsComponent.AdjustHappiness(6f);
        targetCharacter.needsComponent.AdjustHappiness(6f);
        //goapNode.actor.needsComponent.AdjustStamina(1f);
        //targetCharacter.needsComponent.AdjustStamina(1f);
    }
    public void AfterMakeLoveSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        Character targetCharacter = goapNode.poiTarget as Character;
        Bed bed = null;
        if (actor.tileObjectComponent.primaryBed != null) {
            if (actor.tileObjectComponent.primaryBed.gridTileLocation != null
                && (actor.gridTileLocation == actor.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(actor.tileObjectComponent.primaryBed.gridTileLocation, true))) {
                bed = actor.tileObjectComponent.primaryBed;
            }
        } else if (targetCharacter.tileObjectComponent.primaryBed != null) {
            if (targetCharacter.tileObjectComponent.primaryBed.gridTileLocation != null
                && (actor.gridTileLocation == targetCharacter.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(targetCharacter.tileObjectComponent.primaryBed.gridTileLocation, true))) {
                bed = targetCharacter.tileObjectComponent.primaryBed;
            }
        }
        //Bed bed = goapNode.actor.gridTileLocation.structure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED).FirstOrDefault() as Bed;
        bed?.OnDoneActionToObject(goapNode);
        //goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        //targetCharacter.needsComponent.AdjustDoNotGetBored(-1);

        //**After Effect 1**: If Actor and Target are Lovers, they both gain Cheery trait. If Actor and Target are Affairs, they both gain Ashamed trait.
        if (actor is SeducerSummon) {
            //kill the target character
            targetCharacter.Death("seduced", goapNode, actor);
        }

        //if (goapNode.actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER)) {
        //    goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Satisfied", targetCharacter);
        //    targetCharacter.traitContainer.AddTrait(targetCharacter, "Satisfied", goapNode.actor);
        //} else if (goapNode.actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.AFFAIR)) {
        //    goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Ashamed", targetCharacter);
        //    targetCharacter.traitContainer.AddTrait(targetCharacter, "Ashamed", goapNode.actor);
        //}
        //goapNode.actor.ownParty.RemovePOI(targetCharacter);
        //targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Wooed");

        //targetCharacter.RemoveTargettedByAction(this);
        if (targetCharacter.currentActionNode == goapNode) {
            targetCharacter.SetCurrentActionNode(null, null, null);
        }
    }
#endregion

#region Preconditions
    private bool IsTargetInvited(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.carryComponent.IsPOICarried(poiTarget);
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area)) {
                return false;
            }
            Character target = poiTarget as Character;
            if (target == actor) {
                return false;
            }
            //if (target.currentAlterEgoName != CharacterManager.Original_Alter_Ego) { //do not woo characters that have transformed to other alter egos
            //    return false;
            //}
            if (!target.limiterComponent.canPerform) { //target.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
                return false;
            }
            if (target.stateComponent.currentState is CombatState) { //do not invite characters that are currently in combat
                return false;
            }
            if (target.hasBeenRaisedFromDead) { //do not woo characters that have been raised from the dead
                return false;
            }
            if (target.carryComponent.masterCharacter.movementComponent.isTravellingInWorld || target.currentRegion != actor.currentRegion) {
                return false; //target is outside the map
            }
            if (GetValidBedForActor(actor, target) == null) {
                return false;/**/
            }
            if (!(actor is SeducerSummon)) { //ignore relationships if succubus
                if (!actor.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.LOVER) && !actor.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.AFFAIR)) {
                    return false; //only lovers and affairs can make love
                }
            }
            return true;
        }
        return false;
    }
#endregion

    private Bed GetValidBedForActor(Character actor, [NotNull]Character target) {
        Bed bedToUse = null;
        if(actor.tileObjectComponent.primaryBed != null) {
            bedToUse = actor.tileObjectComponent.primaryBed;
        } else if (target.tileObjectComponent.primaryBed != null) {
            bedToUse = target.tileObjectComponent.primaryBed;
        }
        return bedToUse;
        //if (actor is Summon) {
        //    //check un owned dwellings for possible beds
        //    //Bed nearestBed = null;
        //    //if (target.homeSettlement != null) {
        //    //    List<Bed> beds = target.homeSettlement.GetTileObjectsOfTypeThatMeetCriteria<Bed>(b => b.mapObjectState == MAP_OBJECT_STATE.BUILT && b.IsAvailable() && b.GetActiveUserCount() == 0);
        //    //    float nearestDistance = 0f;
        //    //    for (int i = 0; i < beds.Count; i++) {
        //    //        Bed bed = beds[i];
        //    //        float distanceFromActor = actor.gridTileLocation.GetDistanceTo(bed.gridTileLocation);
        //    //        if (nearestBed == null || distanceFromActor < nearestDistance) {
        //    //            nearestBed = bed;
        //    //            nearestDistance = distanceFromActor;
        //    //        }
        //    //    }
        //    //}
        //    List<Dwelling> dwellings =
        //        actor.currentRegion.GetStructuresAtLocation<Dwelling>(STRUCTURE_TYPE.DWELLING);
        //    Bed nearestBed = null;
        //    float nearestDistance = 0f;
        //    for (int i = 0; i < dwellings.Count; i++) {
        //        Dwelling currDwelling = dwellings[i];
        //        Bed dwellingBed = currDwelling.GetTileObjectOfType<Bed>(TILE_OBJECT_TYPE.BED);
        //        if (dwellingBed != null && dwellingBed.mapObjectState == MAP_OBJECT_STATE.BUILT && dwellingBed.IsAvailable() && dwellingBed.GetActiveUserCount() == 0) {
        //            float distanceFromActor = actor.gridTileLocation.GetDistanceTo(dwellingBed.gridTileLocation);
        //            if (nearestBed == null || distanceFromActor < nearestDistance) {
        //                nearestBed = dwellingBed;
        //                nearestDistance = distanceFromActor;
        //            }
        //        }
        //    }
        //    return nearestBed;
        //} else {
        //    if(actor.homeStructure != null) {
        //        Bed actorBed = actor.homeStructure.GetTileObjectOfType<Bed>(TILE_OBJECT_TYPE.BED);
        //        if (actorBed != null && actorBed.GetActiveUserCount() == 0) {
        //            return actorBed;
        //        } else if (target.homeStructure != null){
        //            Bed targetBed = target.homeStructure.GetTileObjectOfType<Bed>(TILE_OBJECT_TYPE.BED);
        //            if (targetBed != null && targetBed.GetActiveUserCount() == 0) {
        //                return targetBed;
        //            }
        //        }
        //    }
        //    return null;
        //}   
    }
}