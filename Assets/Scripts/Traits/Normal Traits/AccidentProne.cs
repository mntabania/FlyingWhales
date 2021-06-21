using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Traits {
    public class AccidentProne : Trait {

        public Character owner { get; private set; }

        public AccidentProne() {
            name = "Accident Prone";
            description = "A walking and talking disaster.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Start_Perform_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Per_Tick_While_Stationary_Unoccupied);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                owner = sourceCharacter as Character;
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
            }
        }
        public override bool PerTickWhileStationaryOrUnoccupied(Character p_character) {
            if (owner.hasMarker && owner.marker.isMoving) {
                int stumbleChance = UnityEngine.Random.Range(0, 100);
                if (stumbleChance < 2) {
                    return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Stumble, owner);
                }    
            }
            return false;
        }
        public override bool OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction) {
            if (node.goapType == INTERACTION_TYPE.STAND || node.goapType == INTERACTION_TYPE.STAND_STILL || node.goapType == INTERACTION_TYPE.LONG_STAND_STILL) {
                return false;
            }
            int accidentChance = UnityEngine.Random.Range(0, 100);
            //bool hasCreatedJob = false;
            if (accidentChance < 10) {
                willStillContinueAction = false;
                return node.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Accident, node.actor);
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
        //    ActualGoapNode node = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.STUMBLE], owner, owner, null, 0);
        //    GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(node, owner);
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
        //    ActualGoapNode node = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ACCIDENT], owner, owner, new object[] { action }, 0);
        //    GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(node, owner);
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

