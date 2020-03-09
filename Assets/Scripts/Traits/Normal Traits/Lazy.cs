using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Lazy : Trait {
        public Character owner { get; private set; }

        public Lazy() {
            name = "Lazy";
            description = "Lazy characters often daydream and are less likely to take on npcSettlement tasks.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            mutuallyExclusive = new string[] { "Hardworking" };
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                owner = addedTo as Character;
            }
        }
        // public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        //     base.OnRemoveTrait(removedFrom, removedBy);
        //     owner.SetIsLazy(false);
        // }
        public override string TriggerFlaw(Character character) {
            //Will drop current action and will perform Happiness Recovery.
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                if (character.currentActionNode.action != null) {
                    character.StopCurrentActionNode(false);
                }
                if (character.stateComponent.currentState != null) {
                    character.stateComponent.ExitCurrentState();
                }

                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = character.traitContainer.GetNormalTrait<Heartbroken>("Heartbroken");
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < (25 * owner.traitContainer.stacks[heartbroken.name]);
                }
                if (!triggerBrokenhearted) {
                    if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY)) {
                        character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
                    }
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, target = GOAP_EFFECT_TARGET.ACTOR }, character, character);
                    character.jobQueue.AddJobInQueue(job);
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
            }
            return base.TriggerFlaw(character);
        }
        #endregion

        public bool TriggerLazy() {
            return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Lazy, owner);
        }
    }
}

