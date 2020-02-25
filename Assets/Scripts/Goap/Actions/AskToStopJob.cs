using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class AskToStopJob : GoapAction {

    public GoapPlanJob jobToStop { get; private set; }

    public AskToStopJob() : base(INTERACTION_TYPE.ASK_TO_STOP_JOB) {
        actionIconString = GoapActionStateDB.Work_Icon;
        doesNotStopTargetCharacter = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }

   // #region Overrides
   // protected override void ConstructRequirement() {
   //     _requirementAction = Requirement;
   // }
   // protected override void ConstructBasePreconditionsAndEffects() {
   //     AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_STOP_ACTION_AND_JOB, targetPOI = poiTarget });
   // }
   // public override void Perform(ActualGoapNode goapNode) {
   //     base.Perform(goapNode);
   //     if (!isTargetMissing) {
   //         if (poiTarget is Character) {
   //             Character targetCharacter = poiTarget as Character;
   //             if (targetCharacter.currentActionNode != null && targetCharacter.currentActionNode.parentPlan != null && targetCharacter.currentActionNode.parentPlan.job != null
   //             && targetCharacter.currentActionNode.parentPlan.job == jobToStop) {
   //                 SetState("Ask Success");
   //             } else {
   //                 SetState("Ask Fail");
   //             }
   //         }
   //     } else {
   //         SetState("Target Missing");
   //     }
   // }
   // protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
   //     if (poiTarget is Character) {
   //         Character targetCharacter = poiTarget as Character;
   //         if(targetCharacter.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
   //             return Utilities.rng.Next(15, 25);
   //         }
   //     }
   //     return Utilities.rng.Next(20, 45);
   // }
   // public override bool InitializeOtherData(object[] otherData) {
   //     this.otherData = otherData;
   //     if (otherData.Length == 1 && otherData[0] is GoapPlanJob) {
   //         jobToStop = otherData[0] as GoapPlanJob;
   //         return true;
   //     }
   //     return base.InitializeOtherData(otherData);
   // }
   // #endregion

   // #region Requirements
   //protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
   //     bool targetDoesNotConsiderActorEnemy = true;
   //     //bool canDoAction = false;
   //     if (poiTarget is Character) {
   //         Character targetCharacter = poiTarget as Character;
   //         //if(jobToStop == null) {
   //         //    canDoAction = true;
   //         //} else if (targetCharacter.currentAction != null && targetCharacter.currentAction.parentPlan != null && targetCharacter.currentAction.parentPlan.job != null
   //         //    && targetCharacter.currentAction.parentPlan.job == jobToStop) {
   //         //    canDoAction = true;
   //         //}
   //         targetDoesNotConsiderActorEnemy = !targetCharacter.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY);
   //     }
   //     return actor != poiTarget && targetDoesNotConsiderActorEnemy; //&& canDoAction
   // }
   // #endregion

   // #region State Effects
   // public void PreAskSuccess() {
   //     if (poiTarget is Character) {
   //         Character targetCharacter = poiTarget as Character;
   //         goapNode.descriptionLog.AddToFillers(null, targetCharacter.currentActionNode.goapName, LOG_IDENTIFIER.STRING_1);
   //     }
   // }
   // public void PreAskFail() {
   //     if (poiTarget is Character) {
   //         Character targetCharacter = poiTarget as Character;
   //         goapNode.descriptionLog.AddToFillers(null, jobToStop.name, LOG_IDENTIFIER.STRING_1);
   //     }
   // }
   // public void AfterAskSuccess() {
   //     if (poiTarget is Character) {
   //         Character targetCharacter = poiTarget as Character;
   //         if(targetCharacter.currentActionNode != null) {
   //             targetCharacter.currentActionNode.StopAction(true);
   //         }
   //     }
   // }
   // #endregion
}

public class AskToStopJobData : GoapActionData {
    public AskToStopJobData() : base(INTERACTION_TYPE.ASK_TO_STOP_JOB) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool targetDoesNotConsiderActorEnemy = true;
        if (poiTarget is Character) {
            Character targetCharacter = poiTarget as Character;
            targetDoesNotConsiderActorEnemy = !targetCharacter.relationshipContainer.IsEnemiesWith(actor);
        }
        return actor != poiTarget && targetDoesNotConsiderActorEnemy;
    }
}