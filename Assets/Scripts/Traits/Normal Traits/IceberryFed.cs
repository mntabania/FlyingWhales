using System;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class IceberryFed : Status {
        
        public IceberryFed() {
            name = "Iceberry Fed";
            description = "Recently ate: Iceberry. Doubles all elemental resistances";
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
                //Fire, Water, Earth, Wind
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Fire, 2);
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Water, 2);
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Earth, 2);
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Wind, 2);
            }
        }
        
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character targetCharacter) {
                //Fire, Water, Earth, Wind
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Fire, -2);
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Water, -2);
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Earth, -2);
                targetCharacter.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Wind, -2);
            }
        }
        #endregion
    }
}