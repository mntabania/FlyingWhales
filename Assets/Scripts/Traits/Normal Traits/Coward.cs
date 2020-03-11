using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class Coward : Trait {

        public Coward() {
            name = "Coward";
            description = "Cowards always flee from combat.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override string TriggerFlaw(Character character) {
            //If outside and the character lives in a house, the character will flee and go back home.
            string successLogKey = base.TriggerFlaw(character);
            if (character.homeStructure != null) {
                if (character.currentStructure != character.homeStructure) {
                    if (character.currentActionNode.action != null) {
                        character.StopCurrentActionNode(false);
                    }
                    if (character.stateComponent.currentState != null) {
                        character.stateComponent.ExitCurrentState();
                    } 

                    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], character, character, null, 0);
                    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, character);
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.RETURN_HOME, character, character);
                    goapPlan.SetDoNotRecalculate(true);
                    job.SetCannotBePushedBack(true);
                    job.SetAssignedPlan(goapPlan);
                    character.jobQueue.AddJobInQueue(job);
                    return successLogKey;
                } else {
                    return "fail_at_home";
                }
            } else {
                return "fail_no_home";
            }
        }
        #endregion
    }

}
