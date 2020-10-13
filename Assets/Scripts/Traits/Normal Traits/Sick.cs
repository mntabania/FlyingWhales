using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Sick : Status {
        private Character owner;
        private readonly float pukeChance;

        public Sick() {
            name = "Sick";
            description = "Has a mild illness.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            mutuallyExclusive = new string[] { "Robust" };
            moodEffect = -4;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.5f;
            hindersSocials = true;
            pukeChance = 5f;
            AddTraitOverrideFunctionIdentifier(TraitManager.Per_Tick_Movement);
        }

        #region Loading
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character) {
                owner = addTo as Character;
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                owner = sourceCharacter as Character;
                owner.movementComponent.AdjustSpeedModifier(-0.10f);
                owner.AddTraitNeededToBeRemoved(this);

                if (gainedFromDoing == null || gainedFromDoing.goapType != INTERACTION_TYPE.EAT) {
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait", gainedFromDoing, LOG_TAG.Misc);
                    // if (gainedFromDoing != null) {
                    //     addLog.SetLogType(LOG_TYPE.Action);
                    // }
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(null, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddLogToDatabase();
                }
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            owner.movementComponent.AdjustSpeedModifier(0.10f);
            owner.RemoveTraitNeededToBeRemoved(this);
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "remove_trait", null, LOG_TAG.Misc);
            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddLogToDatabase();
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override bool PerTickOwnerMovement() {
            float pukeRoll = Random.Range(0f, 100f);
            if (pukeRoll < pukeChance) {
                //do puke action
                if (owner.characterClass.className == "Zombie") {
                    return false;
                }
                return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, owner, "Sick");
            }
            return false;
        }
        #endregion
    }
}

