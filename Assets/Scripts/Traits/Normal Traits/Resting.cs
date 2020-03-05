using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Resting : Trait {
        public override bool isNotSavable {
            get { return true; }
        }

        private Character _character;
        public Resting() {
            name = "Resting";
            description = "This character is resting.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            //hindersMovement = true;
            hindersWitness = true;
            hindersPerform = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            if (sourceCharacter is Character) {
                _character = sourceCharacter as Character;
                //Messenger.AddListener(Signals.TICK_STARTED, RecoverHP);
            }
            base.OnAddTrait(sourceCharacter);
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            //Messenger.RemoveListener(Signals.TICK_STARTED, RecoverHP);
            _character = null;
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override void OnTickStarted() {
            base.OnTickStarted();
            RecoverHP();
        }
        public override void OnHourStarted() {
            base.OnHourStarted();
            CheckForLycanthropy();
        }
        #endregion

        private void RecoverHP() {
            _character.HPRecovery(0.02f);
        }

        private void CheckForLycanthropy() {
            if(_character.isLycanthrope) {
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < 25) {
                    _character.lycanData.Transform(_character);
                }
            }
            
            //TODO:
            //if (restingTrait.lycanthropyTrait == null) {
            //    if (currentState.currentDuration == currentState.duration) {
            //        //If sleep will end, check if the actor is being targetted by Drink Blood action, if it is, do not end sleep
            //        bool isTargettedByDrinkBlood = false;
            //        for (int i = 0; i < actor.targettedByAction.Count; i++) {
            //            if (actor.targettedByAction[i].goapType == INTERACTION_TYPE.DRINK_BLOOD && !actor.targettedByAction[i].isDone && actor.targettedByAction[i].isPerformingActualAction) {
            //                isTargettedByDrinkBlood = true;
            //                break;
            //            }
            //        }
            //        if (isTargettedByDrinkBlood) {
            //            currentState.OverrideDuration(currentState.duration + 1);
            //        }
            //    }
            //} else {
            //    bool isTargettedByDrinkBlood = false;
            //    for (int i = 0; i < actor.targettedByAction.Count; i++) {
            //        if (actor.targettedByAction[i].goapType == INTERACTION_TYPE.DRINK_BLOOD && !actor.targettedByAction[i].isDone && actor.targettedByAction[i].isPerformingActualAction) {
            //            isTargettedByDrinkBlood = true;
            //            break;
            //        }
            //    }
            //    if (currentState.currentDuration == currentState.duration) {
            //        //If sleep will end, check if the actor is being targetted by Drink Blood action, if it is, do not end sleep
            //        if (isTargettedByDrinkBlood) {
            //            currentState.OverrideDuration(currentState.duration + 1);
            //        } else {
            //            if (!restingTrait.hasTransformed) {
            //                restingTrait.CheckForLycanthropy(true);
            //            }
            //        }
            //    } else {
            //        if (!isTargettedByDrinkBlood) {
            //            restingTrait.CheckForLycanthropy();
            //        }
            //    }
            //}
        }
    }
}

