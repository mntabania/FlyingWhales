using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Nocturnal : Trait {
        public override bool isSingleton => true;

        public Nocturnal() {
            name = "Nocturnal";
            description = "Nocturnals are awake at night and asleep during the day.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                character.needsComponent.SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.EARLY_NIGHT);
                character.needsComponent.SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS.MORNING);
                character.needsComponent.SetFullnessForcedTick();
                character.needsComponent.SetTirednessForcedTick();
            }
        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            base.OnRemoveTrait(sourcePOI, removedBy);
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                character.needsComponent.SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.LUNCH_TIME);
                character.needsComponent.SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS.LATE_NIGHT);
                character.needsComponent.SetFullnessForcedTick();
                character.needsComponent.SetTirednessForcedTick();
            }
        }
        #endregion
    }
}

