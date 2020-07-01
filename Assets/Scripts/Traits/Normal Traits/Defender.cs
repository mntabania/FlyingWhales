using System;
namespace Traits {
    public class Defender : Trait {
        
        public override bool isSingleton => true;
        
        public Defender() {
            name = "Defender";
            description = "This is a Defender.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                character.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Defender_Behaviour);
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