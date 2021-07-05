using System;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class PineappleFed : Status {
        
        public PineappleFed() {
            name = "Pineapple Fed";
            description = "Recently ate: Pineapple. Doubles mental and physical resistances";
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
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Physical, 2);
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Mental, 2);
            }
        }
        
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character targetCharacter) {
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Physical, -2);
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Mental, -2);
            }
        }
        #endregion
    }
}