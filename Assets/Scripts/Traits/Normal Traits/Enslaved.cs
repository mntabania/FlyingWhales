using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Enslaved : Status {
        public override bool isSingleton => true;

        public Enslaved() {
            name = "Enslaved";
            description = "Forced to gather food for its master.";
            thoughtText = "I miss freedom.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            moodEffect = -8;
            hindersSocials = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character targetCharacter) {
                if (targetCharacter.isNotSummonAndDemon) {
                    targetCharacter.AssignClass("Peasant");
                }
            }
        }
        //public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
        //    base.OnRemoveTrait(sourcePOI, removedBy);
        //    if (sourcePOI is Character targetCharacter) {
        //    }
        //}
        #endregion
    }
}

