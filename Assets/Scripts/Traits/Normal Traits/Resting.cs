using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Resting : Status {
        public override bool isSingleton => true;

        //private Character _character;
        public Resting() {
            name = "Resting";
            description = "Sleeping. May or may not be snoring.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            //hindersMovement = true;
            hindersWitness = true;
            hindersPerform = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Hour_Started_Trait);
        }

        #region Overrides
        //public override void OnAddTrait(ITraitable sourceCharacter) {
        //    if (sourceCharacter is Character) {
        //        _character = sourceCharacter as Character;
        //        //Messenger.AddListener(Signals.TICK_STARTED, RecoverHP);
        //    }
        //    base.OnAddTrait(sourceCharacter);
        //}
        //public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        //    //Messenger.RemoveListener(Signals.TICK_STARTED, RecoverHP);
        //    _character = null;
        //    base.OnRemoveTrait(sourceCharacter, removedBy);
        //}
        public override void OnTickStarted(ITraitable traitable) {
            base.OnTickStarted(traitable);
            if (traitable is Character character) {
                RecoverHP(character);
            }
        }
        public override void OnHourStarted(ITraitable traitable) {
            base.OnHourStarted(traitable);
            if(traitable is Character character) {
                CheckForLycanthropy(character);
            }
        }
        #endregion

        private void RecoverHP(Character character) {
            character.HPRecovery(1); //0.02
        }

        private void CheckForLycanthropy(Character character) {
            if(character.isLycanthrope && !character.lycanData.isMaster && character.carryComponent.isBeingCarriedBy == null) {
                if (ChanceData.RollChance(CHANCE_TYPE.Lycanthrope_Transform_Chance)) {
                    character.lycanData.Transform(character);
                }
            }
        }
    }
}

