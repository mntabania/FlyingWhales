using System;
namespace Traits {
    public class Disabler : Trait {
        
        public override bool isSingleton => true;
        
        public Disabler() {
            name = "Disabler";
            description = "This is a Disabler.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                character.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Disabler_Behaviour);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.behaviourComponent.UpdateDefaultBehaviourSet();
            }
        }
        #endregion
    }
}