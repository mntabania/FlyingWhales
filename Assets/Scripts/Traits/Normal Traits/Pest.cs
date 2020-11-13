using System;
namespace Traits {
    public class Pest : Trait {
        
        public override bool isSingleton => true;
        
        public Pest() {
            name = "Pest";
            description = "Eats crops and other food piles.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                character.movementComponent.SetEnableDigging(true);
                character.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Pest_Behaviour);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.movementComponent.SetEnableDigging(false);
                character.behaviourComponent.UpdateDefaultBehaviourSet();
            }
        }
        #endregion
    }
}