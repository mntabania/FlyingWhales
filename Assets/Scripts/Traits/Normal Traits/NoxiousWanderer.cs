using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class NoxiousWanderer : Trait {
        public override bool isSingleton => true;

        public NoxiousWanderer() {
            name = "Noxious Wanderer";
            description = "This is a noxious wanderer.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.SPAWN_POISON_CLOUD };
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                character.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Noxious_Wanderer_Behaviour);
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

