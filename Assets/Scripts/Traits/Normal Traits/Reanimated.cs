using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Reanimated : Status {

        public Reanimated() {
            name = "Reanimated";
            description = "Brought back to life by some unholy magic.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.POSITIVE;
            
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            base.OnRemoveTrait(sourcePOI, removedBy);
            if (sourcePOI is Character) {
                (sourcePOI as Character).Death(); //when a character with a reanimated trait has the reanimated trait removed from him/her, he/she dies again
            }
        }
        #endregion
    }
}

