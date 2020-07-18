using System;
namespace Traits {
    public class DeMooder : Trait {
        
        public override bool isSingleton => true;
        
        public DeMooder() {
            name = "De-Mooder";
            description = "Applies a temporary debuff on enemies that significantly reduces their Mood.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                character.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.DeMooder_Behaviour);
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