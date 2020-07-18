using System;
namespace Traits {
    public class Arsonist : Trait {
        
        public override bool isSingleton => true;
        
        public Arsonist() {
            name = "Arsonist";
            description = "Burns down nearby objects, structures and villagers";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                character.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Arsonist_Behaviour);
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