using UnityEngine.Assertions;
using UnityEngine;
using System;
using Traits;
using UtilityScripts;
using Random = UnityEngine.Random;

namespace Traits {
    public class WalkerZombie : FiniteZombie {

        public WalkerZombie() {
            name = "Walker Zombie";
            description = "Walker Zombie";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Override
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (owner != null) {
                SetMovementSpeed();
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (owner != null) {
                UnsetMovementSpeed();
            }
        }
        #endregion

        private void SetMovementSpeed() {
            owner.movementComponent.AdjustRunSpeedModifier(-0.2f);
            owner.movementComponent.AdjustWalkSpeedModifier(-0.5f);
        }
        private void UnsetMovementSpeed() {
            owner.movementComponent.AdjustRunSpeedModifier(0.2f);
            owner.movementComponent.AdjustWalkSpeedModifier(0.5f);
        }
    }
}