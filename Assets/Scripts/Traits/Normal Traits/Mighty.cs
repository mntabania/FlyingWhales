using System.Collections.Generic;

namespace Traits {
    public class Mighty : Trait {
        public override bool isSingleton => true;

        public Mighty() {
            name = "Mighty";
            description = "This character is mighty.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.POSITIVE;
            ticksDuration = 0;
            canBeTriggered = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                character.combatComponent.AdjustMaxHPModifier(character.combatComponent.unModifiedMaxHP);
                character.combatComponent.AdjustAttackModifier(character.combatComponent.unModifiedAttack);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.combatComponent.AdjustMaxHPModifier(-character.combatComponent.unModifiedMaxHP);
                character.combatComponent.AdjustAttackModifier(-character.combatComponent.unModifiedAttack);
            }
        }
        #endregion
    }
}

