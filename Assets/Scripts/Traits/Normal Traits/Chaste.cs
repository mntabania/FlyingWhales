using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Chaste : Trait {

        public Chaste() {
            name = "Chaste";
            description = "Character is less likely to engage in lovemaking.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            mutuallyExclusive = new string[] { "Lustful" };
        }

        #region Overrides
        public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, object[] otherData, ref int cost) {
            base.ExecuteCostModification(action, actor, poiTarget, otherData, ref cost);
            if (action == INTERACTION_TYPE.MAKE_LOVE) {
                cost = UtilityScripts.Utilities.rng.Next(40, 67);
            }
        }
        //public override void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) {
        //    base.ExecuteActionPerTickEffects(action, goapNode);
        //    if (action == INTERACTION_TYPE.MAKE_LOVE) {
        //        goapNode.actor.needsComponent.AdjustHappiness(-100);
        //    }
        //}
        #endregion


    }
}
