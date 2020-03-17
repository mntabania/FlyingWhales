using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Traits {
    public class AccidentProne : Trait {

        //public static INTERACTION_TYPE[] excludedActionsFromAccidentProneTrait = new INTERACTION_TYPE[] {
        //    INTERACTION_TYPE.STUMBLE, INTERACTION_TYPE.PUKE, INTERACTION_TYPE.SEPTIC_SHOCK, INTERACTION_TYPE.ACCIDENT
        //};

        public Character owner { get; private set; }
        public CharacterState storedState { get; private set; }

        public AccidentProne() {
            name = "Accident Prone";
            description = "Accident Prone characters often gets injured.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Start_Perform_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                owner = sourceCharacter as Character;
            }
        }
        public override bool PerTickOwnerMovement() {
            int stumbleChance = UnityEngine.Random.Range(0, 100);
            //bool hasCreatedJob = false;
            if (stumbleChance < 2) {
                return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Stumble, owner);
                //if (owner.currentActionNode == null || (owner.currentActionNode.action.goapType != INTERACTION_TYPE.STUMBLE && owner.currentActionNode.action.goapType != INTERACTION_TYPE.ACCIDENT)) {
                //    DoStumble();
                //    hasCreatedJob = true;
                //}
            }
            return false;
        }
        public override bool OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction) {
            int accidentChance = UnityEngine.Random.Range(0, 100);
            //bool hasCreatedJob = false;
            if (accidentChance < 10) {
                willStillContinueAction = false;
                return node.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Accident, node.actor);
                //if (node != null && !excludedActionsFromAccidentProneTrait.Contains(node.action.goapType)) {
                //    DoAccident(node.action);
                //    hasCreatedJob = true;
                //    willStillContinueAction = false;
                //}
            }
            return false;
        }
        public override string TriggerFlaw(Character character) {
            if (character.marker.isMoving) {
                //If moving, the character will stumble and get injured.
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Stumble, owner);
            } else if (character.currentActionNode != null /*&& !excludedActionsFromAccidentProneTrait.Contains(character.currentActionNode.action.goapType)*/) {
                //If doing something, the character will fail and get injured.
                //DoAccident(character.currentActionNode.action);
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Accident, owner);
            }
            return base.TriggerFlaw(character);
        }
        #endregion

        //private void DoStumble() {
        //    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.STUMBLE], owner, owner, null, 0);
        //    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
        //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INTERRUPTION, INTERACTION_TYPE.STUMBLE, owner, owner);
        //    goapPlan.SetDoNotRecalculate(true);
        //    job.SetCannotBePushedBack(true);
        //    job.SetAssignedPlan(goapPlan);
        //    owner.jobQueue.AddJobInQueue(job);
        //    //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INTERRUPTION, INTERACTION_TYPE.STUMBLE, owner, owner);
        //    //job.SetCannotBePushedBack(true);
        //    //owner.jobQueue.AddJobInQueue(job);
        //}

        //private void DoAccident(GoapAction action) {
        //    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ACCIDENT], owner, owner, new object[] { action }, 0);
        //    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
        //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INTERRUPTION, INTERACTION_TYPE.ACCIDENT, owner, owner);
        //    goapPlan.SetDoNotRecalculate(true);
        //    job.SetCannotBePushedBack(true);
        //    job.SetAssignedPlan(goapPlan);
        //    owner.jobQueue.AddJobInQueue(job);

        //    //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INTERRUPTION, INTERACTION_TYPE.ACCIDENT, owner, owner);
        //    //job.AddOtherData(INTERACTION_TYPE.ACCIDENT, new object[] { action });
        //    //job.SetCannotBePushedBack(true);
        //    //owner.jobQueue.AddJobInQueue(job);
        //}
    }
}

