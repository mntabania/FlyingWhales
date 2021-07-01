using System;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class CornFed : Status {
        
        public CornFed() {
            name = "Corn Fed";
            description = "Recently ate: Corn. Increased Strength by 50%.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.POSITIVE;
            isStacking = true;
            stackLimit = 1;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character targetCharacter) {
                targetCharacter.combatComponent.AdjustStrengthPercentModifier(50);
            }
        }
        
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character targetCharacter) {
                targetCharacter.combatComponent.AdjustStrengthPercentModifier(-50);
            }
        }
        #endregion
    }
}