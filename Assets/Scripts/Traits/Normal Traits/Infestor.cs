using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Infestor : Trait {
        public override bool isSingleton => true;

        public Infestor() {
            name = "Infestor";
            description = "Grows and multiplies over time.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.LAY_EGG };
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                character.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Infestor_Behaviour);
            }
        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            base.OnRemoveTrait(sourcePOI, removedBy);
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                character.behaviourComponent.UpdateDefaultBehaviourSet();
            }
        }
        #endregion
    }
}

