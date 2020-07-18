using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Tower : Trait {
        public override bool isSingleton => true;

        public Tower() {
            name = "Tower";
            description = "This is a tower.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                character.reactionComponent.SetIsHidden(false);
                character.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Tower_Behaviour);
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

