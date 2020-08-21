using System;
namespace Traits {
    public class Subterranean : Trait {
        
        public override bool isSingleton => true;
        
        public Subterranean() {
            name = "Subterranean";
            description = "Can travel underground.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                character.behaviourComponent.AddBehaviourComponent(typeof(SubterraneanBehaviour));
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.behaviourComponent.RemoveBehaviourComponent(typeof(SubterraneanBehaviour));
            }
        }
        #endregion
    }
}