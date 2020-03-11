using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Traits {
    public class Drunkard : Trait {

        private bool hasDrankWithinTheDay;
        private Character owner;

        public Drunkard() {
            name = "Drunkard";
            description = "Drunkards enjoy drinking.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            canBeTriggered = true;
            hasDrankWithinTheDay = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if(addedTo is Character) {
                owner = addedTo as Character;
            }
            Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
            Messenger.AddListener<ActualGoapNode>(Signals.ACTION_PERFORMED, OnPerformAction);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
            Messenger.AddListener<ActualGoapNode>(Signals.ACTION_PERFORMED, OnPerformAction);
            base.OnRemoveTrait(removedFrom, removedBy);
        }
        public override string TriggerFlaw(Character character) {
            //Will drink
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = character.traitContainer.GetNormalTrait<Heartbroken>("Heartbroken");
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < (25 * owner.traitContainer.stacks[heartbroken.name]);
                }
                if (!triggerBrokenhearted) {
                    if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY)) {
                        character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
                    }

                    //TileObject to = character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.INN).GetTileObjectsThatAdvertise(INTERACTION_TYPE.DRINK).First();
                    GoapPlanJob drinkJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.DRINK, character, character);
                    character.jobQueue.AddJobInQueue(drinkJob);
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
            }
            return base.TriggerFlaw(character);
        }
        public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, object[] otherData, ref int cost) {
            base.ExecuteCostModification(action, actor, poiTarget, otherData, ref cost);
            if (action == INTERACTION_TYPE.DRINK) {
                cost =  UtilityScripts.Utilities.rng.Next(5, 20);
            }
        }
        #endregion

        private void OnDayStarted() {
            if (!hasDrankWithinTheDay) {
                owner.traitContainer.AddTrait(owner, "Withdrawal");
            }
            hasDrankWithinTheDay = false;
        }
        private void OnPerformAction(ActualGoapNode node) {
            if(node.action.goapType == INTERACTION_TYPE.DRINK) {
                if (!hasDrankWithinTheDay) {
                    hasDrankWithinTheDay = true;
                }
            }
        }
    }

}