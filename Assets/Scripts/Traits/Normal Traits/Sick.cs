using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Sick : Status {
        private Character owner;
        private float pukeChance;
        //public override bool isRemovedOnSwitchAlterEgo {
        //    get { return true; }
        //}
        public Sick() {
            name = "Sick";
            description = "This character has caught a mild illness.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CURE_CHARACTER, };
            mutuallyExclusive = new string[] { "Robust" };
            moodEffect = -4;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.5f;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                owner = sourceCharacter as Character;
                owner.movementComponent.AdjustSpeedModifier(-0.10f);
                //_sourceCharacter.CreateRemoveTraitJob(name);
                owner.AddTraitNeededToBeRemoved(this);
                //owner.needsComponent.AdjustStaminaDecreaseRate(5);

                if (gainedFromDoing == null) {
                    owner.RegisterLog("NonIntel", "add_trait", null, name.ToLower());
                } else {
                    if (gainedFromDoing.goapType == INTERACTION_TYPE.EAT) {
                        Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait", gainedFromDoing);
                        if (gainedFromDoing != null) {
                            addLog.SetLogType(LOG_TYPE.Action);
                        }
                        addLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        //TODO: gainedFromDoing.states["Eat Poisoned"].AddArrangedLog("sick", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, owner, true));
                    } else {
                        owner.RegisterLog("NonIntel", "add_trait", null, name.ToLower());
                    }
                }
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            owner.movementComponent.AdjustSpeedModifier(0.10f);
            owner.RemoveTraitNeededToBeRemoved(this);
            //owner.needsComponent.AdjustStaminaDecreaseRate(-5);
            owner.RegisterLog("NonIntel", "remove_trait", null, name.ToLower());
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        protected override void OnChangeLevel() {
            if (level == 1) {
                pukeChance = 5f;
            } else if (level == 2) {
                pukeChance = 7f;
            } else {
                pukeChance = 9f;
            }
        }
        public override bool PerTickOwnerMovement() {
            float pukeRoll = Random.Range(0f, 100f);
            if (pukeRoll < pukeChance) {
                //do puke action
                if (owner.characterClass.className == "Zombie" /*|| (owner.currentActionNode != null && owner.currentActionNode.action.goapType == INTERACTION_TYPE.PUKE)*/) {
                    return false;
                }
                //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE, owner, owner);
                //owner.jobQueue.AddJobInQueue(job);
                return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, owner);
            }
            return false;
        }
        #endregion
    }
}

