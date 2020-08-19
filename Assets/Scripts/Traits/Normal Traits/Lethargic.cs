using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Lethargic : Status {
        public override bool isSingleton => true;

        public Lethargic() {
            name = "Lethargic";
            description = "Moving very sluggishly.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
            moodEffect = -4;
            //effects = new List<TraitEffect>();
            //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TRANSFORM_TO_WOLF, INTERACTION_TYPE.REVERT_TO_NORMAL };
            hindersSocials = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                Character character = sourceCharacter as Character;
                character.movementComponent.AdjustSpeedModifier(-0.5f);
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (sourceCharacter is Character) {
                Character character = sourceCharacter as Character;
                character.movementComponent.AdjustSpeedModifier(0.5f);
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        #endregion
    }
}

