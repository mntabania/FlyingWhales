using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Infected : Status {

        private Character owner;
        //private float pukeChance;
        //private bool canBeReanimated;
        //private bool willBeReanimated;
        private bool doNotCheckPerHour;

        public override bool isPersistent { get { return true; } }
        public Infected() {
            name = "Infected";
            description = "This character has the zombie virus.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CURE_CHARACTER, };
            mutuallyExclusive = new string[] { "Robust" };
            //canBeReanimated = false;
            //willBeReanimated = false;
            doNotCheckPerHour = false;
            moodEffect = -5;
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Hour_Started_Trait);
        }

        #region Override
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            owner = addedTo as Character;
            owner.needsComponent.AdjustComfortDecreaseRate(10);
            //Messenger.AddListener(Signals.HOUR_STARTED, PerHour);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            owner.marker.SetMarkerColor(Color.white);
            owner.needsComponent.AdjustComfortDecreaseRate(-10);
        }
        public override bool OnDeath(Character character) {
            if (character.characterClass.className == "Zombie") {
                //if the character that died is a zombie, remove this trait
                Messenger.RemoveListener<Character, Character>(Signals.CHARACTER_WAS_HIT, OnCharacterHit);
                return owner.traitContainer.RemoveTrait(owner, this);
            } else {
                //Messenger.RemoveListener(Signals.HOUR_STARTED, PerHour);
                doNotCheckPerHour = true;
                SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(30)), StartReanimationCheck, this);
            }
            return base.OnDeath(character);
        }
        //protected override void OnChangeLevel() {
        //    if (level == 1) {
        //        pukeChance = 5f;
        //    } else if (level == 2) {
        //        pukeChance = 7f;
        //    } else {
        //        pukeChance = 9f;
        //    }
        //}
        //public override bool PerTickOwnerMovement() {
        //    float pukeRoll = Random.Range(0f, 100f);
        //    if (pukeRoll < pukeChance) {
        //        //do puke action
        //        if (owner.characterClass.className == "Zombie"/* || (owner.currentActionNode != null && owner.currentActionNode.action.goapType == INTERACTION_TYPE.PUKE)*/) {
        //            //If current action is a roaming action like Hunting To Drink Blood, we must requeue the job after it is removed by StopCurrentAction
        //            return false;
        //        }
        //        //ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PUKE], owner, owner, null, 0);
        //        //GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
        //        //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE, owner, owner);
        //        //goapPlan.SetDoNotRecalculate(true);
        //        //job.SetCannotBePushedBack(true);
        //        //job.SetAssignedPlan(goapPlan);
        //        //owner.jobQueue.AddJobInQueue(job);
        //        ////GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE, owner, owner);
        //        ////owner.jobQueue.AddJobInQueue(job);

        //        return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, owner);
        //    }
        //    return false;
        //}
        //public override void OnTickEnded() {
        //    base.OnTickEnded();
        //    if (canBeReanimated) {
        //        RollForReanimation();
        //    }
        //    if (willBeReanimated) {
        //        CheckIfCanReanimate();
        //    }
        //}
        public override void OnHourStarted() {
            base.OnHourStarted();
            if(!doNotCheckPerHour) {
                PerHour();
            }
        }
        #endregion

        private void PerHour() {
            int roll = Random.Range(0, 100);
            if (roll < 5) { //2 // && owner.isAtHomeRegion
                //owner.marker.StopMovement();
                //if (owner.currentActionNode != null && owner.currentActionNode.action.goapType != INTERACTION_TYPE.ZOMBIE_DEATH) {
                //    owner.StopCurrentActionNode(false);
                //} else if (owner.stateComponent.currentState != null) {
                //    owner.stateComponent.ExitCurrentState();
                //}
                //ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ZOMBIE_DEATH], owner, owner, null, 0);
                //GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
                //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.ZOMBIE_DEATH, owner, owner);
                //goapPlan.SetDoNotRecalculate(true);
                //job.SetCannotBePushedBack(true);
                //job.SetAssignedPlan(goapPlan);
                //owner.jobQueue.AddJobInQueue(job);
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Zombie_Death, owner);
            }
        }

        private void StartReanimationCheck() {
            //canBeReanimated = true;
            Messenger.AddListener(Signals.TICK_ENDED, RollForReanimation);
            //RollForReanimation(); //called this so, check will run immediately after the first 30 mins of being dead.
        }

        private void RollForReanimation() {
            //string summary = owner.name + " will roll for reanimation...";
            if(!owner.marker && owner.grave == null) {
                Messenger.RemoveListener(Signals.TICK_ENDED, RollForReanimation);
                //canBeReanimated = false;
                return;
            }
            int roll = Random.Range(0, 100);
            //summary += "\nRoll is: " + roll.ToString(); 
            if (roll < 15) { //15
                Messenger.RemoveListener(Signals.TICK_ENDED, RollForReanimation);
                //canBeReanimated = false;
                //reanimate
                //summary += "\n" + owner.name + " is being reanimated.";
                if (!owner.IsInOwnParty() || owner.isBeingSeized) {
                    //character is being carried, check per tick if it is dropped or buried, then reanimate
                    Messenger.AddListener(Signals.TICK_ENDED, CheckIfCanReanimate);
                    //willBeReanimated = true;
                } else {
                    Reanimate();
                }

            }
            //Debug.Log(summary);
        }

        private void CheckIfCanReanimate() {
            if (owner.IsInOwnParty() && !owner.isBeingSeized) {
                Reanimate();
                //willBeReanimated = false;
                Messenger.RemoveListener(Signals.TICK_ENDED, CheckIfCanReanimate);
            }
        }

        private void Reanimate() {
            owner.RaiseFromDeath(faction: FactionManager.Instance.zombieFaction, race: owner.race, className: "Zombie");
            owner.marker.SetMarkerColor(Color.grey);
            Messenger.AddListener<Character, Character>(Signals.CHARACTER_WAS_HIT, OnCharacterHit);
        }

        private void OnCharacterHit(Character hitCharacter, Character hitBy) {
            if (hitBy == owner) {
                //a character was hit by the owner of this trait, check if the character that was hit becomes infected.
                string summary = $"{hitCharacter.name} was hit by {hitBy.name}. Rolling for infect...";
                int roll = Random.Range(0, 100);
                summary += $"\nRoll is {roll}";
                if (roll < 20) { //15
                    summary += $"\nChance met, {hitCharacter.name} will turn into a zombie.";
                    if (hitCharacter.traitContainer.AddTrait(hitCharacter, "Infected", characterResponsible: hitBy)) {
                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "contracted_zombie");
                        log.AddToFillers(hitCharacter, hitCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(hitBy, hitBy.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        log.AddLogToInvolvedObjects();
                        // PlayerManager.Instance.player.ShowNotification(log);
                        //Debug.Log(GameManager.Instance.TodayLogString() + Utilities.LogReplacer(log));
                    } else {
                        summary += $"\n{hitCharacter.name} is already a zombie!";
                    }
                }
                Debug.Log(GameManager.Instance.TodayLogString() + summary);
            }
        }
    }
}

