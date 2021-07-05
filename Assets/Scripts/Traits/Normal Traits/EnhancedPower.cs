using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class EnhancedPower : Status {
        public override bool isSingleton => true;

        public EnhancedPower() {
            name = "Enhanced Power";
            description = "Powerrrrr!";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.POSITIVE;
            isHidden = true;
            isStacking = false;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                character.combatComponent.AdjustAttackPercentModifier(20f);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.combatComponent.AdjustAttackPercentModifier(-20f);
            }
        }
        #endregion
    }
}
