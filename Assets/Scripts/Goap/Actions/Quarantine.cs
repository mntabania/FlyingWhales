using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;
public class Quarantine : GoapAction {
    
    public Quarantine() : base(INTERACTION_TYPE.QUARANTINE) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Cure_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.LESSER_DEMON };
        logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Social};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.CARRIED_PATIENT, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), IsPatientCarried);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Quarantine Success", goapNode);
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter);
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        Assert.IsNotNull(targetCharacter, $"Quarantine of {goapNode.actor.name} is not a character! {goapNode.poiTarget?.ToString() ?? "Null"}");
        return GetValidBedForActor(goapNode.actor, targetCharacter);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        if (goapActionInvalidity.isInvalid == false) {
            Character targetCharacter = node.poiTarget as Character;
            Assert.IsNotNull(targetCharacter, $"Quarantine of {node.actor.name} is not a character! {node.poiTarget?.ToString() ?? "Null"}");
            BedClinic targetBed = node.actor.gridTileLocation.tileObjectComponent.objHere as BedClinic;
            if (targetBed == null) {
                //check neighbours
                for (int i = 0; i < node.actor.gridTileLocation.neighbourList.Count; i++) {
                    LocationGridTile neighbour = node.actor.gridTileLocation.neighbourList[i];
                    if (neighbour.tileObjectComponent.objHere is BedClinic bed && bed.IsAvailable() && bed.CanUseBed(targetCharacter)) {
                        targetBed = bed;
                        break;
                    }
                }
            }
            if (targetBed == null) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.reason = "no_space_bed";
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
    }
    #endregion

    #region Preconditions
    private bool IsPatientCarried(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.carryComponent.IsPOICarried(poiTarget);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job) {
        bool requirementsSatisfied = base.AreRequirementsSatisfied(actor, target, otherData, job);
        if (requirementsSatisfied) {
            // Character targetCharacter = target as Character;
            // return GetValidBedForActor(actor, targetCharacter) != null;
            return actor.homeSettlement != null;
        }
        return false;
    }
    #endregion
    
     private BedClinic GetValidBedForActor(Character actor, Character targetCharacter) {
        if (actor.homeSettlement != null) {
            List<LocationStructure> apothecaries = actor.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.HOSPICE);
            if (apothecaries != null) {
                for (int i = 0; i < apothecaries.Count; i++) {
                    LocationStructure structure = apothecaries[i];
                    BedClinic bed = structure.GetFirstBedClinicThatCanBeUsedBy(targetCharacter);
                    if (bed != null) {
                        return bed;
                    }
                }    
            }
        }
        return null;
     }
     private BedClinic GetBedNearActor(Character actor, Character targetCharacter) {
         BedClinic targetBed = actor.gridTileLocation.tileObjectComponent.objHere as BedClinic;
         if (targetBed == null || !targetBed.IsAvailable() || !targetBed.CanUseBed(targetCharacter)) {
             //check neighbours
             for (int i = 0; i < actor.gridTileLocation.neighbourList.Count; i++) {
                 LocationGridTile neighbour = actor.gridTileLocation.neighbourList[i];
                 if (neighbour.tileObjectComponent.objHere is BedClinic bed && bed.IsAvailable() && bed.CanUseBed(targetCharacter)) {
                     targetBed = bed;
                     break;
                 }
             }
         }
         return targetBed;
     }

     #region State Effects
     public void PreQuarantineSuccess(ActualGoapNode goapNode) {
         Character actor = goapNode.actor;
         Character targetCharacter = goapNode.poiTarget as Character;
         Assert.IsNotNull(targetCharacter, $"Target character of Quarantine Action by {goapNode.actor.name} is not a character!");
         BedClinic bed = GetBedNearActor(actor, targetCharacter);
         goapNode.actor.UncarryPOI(targetCharacter, dropLocation: bed.gridTileLocation);
         //If plagued, quarantined indefinitely, else quarantined for 96 hours.
         int overrideDuration = -1;
         if (targetCharacter.traitContainer.HasTrait("Plagued")) {
             overrideDuration = 0;
         }
         targetCharacter.traitContainer.AddTrait(targetCharacter, "Quarantined", overrideDuration: overrideDuration);
         bed.OnDoActionToObject(goapNode);
         targetCharacter.jobQueue.CancelAllJobs("Quarantined");
     }
     #endregion
}
