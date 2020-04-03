using System.Collections.Generic;
namespace Traits {
    public class Dangerous : Status {

        public Dangerous() {
            name = "Dangerous";
            description = "This is dangerous.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.NEUTRALIZE };
            isHidden = true;
        }
        public override bool IsTangible() {
            return true;
        }
    }
}