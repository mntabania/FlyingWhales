using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Dazed : Status {
        // private List<CharacterBehaviourComponent> _behaviourComponentsBeforeDazed;
        public override bool isSingleton => true;

        public Dazed() {
            name = "Dazed";
            description = "Listlessly going back home.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            hindersWitness = true;
            hindersSocials = true;
            // hindersPerform = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                character.behaviourComponent.AddBehaviourComponent(typeof(DazedBehaviour));
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.behaviourComponent.RemoveBehaviourComponent(typeof(DazedBehaviour));
            }
        }
        #endregion
    }
}
