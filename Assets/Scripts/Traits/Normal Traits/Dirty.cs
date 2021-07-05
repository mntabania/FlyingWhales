using System.Collections.Generic;
namespace Traits {
    public class Dirty : Status {

        public Dirty() {
            name = "Dirty";
            description = "Disgustingly filthy.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            isStacking = true;
            stackLimit = 1;
            stackModifier = 1f;
            moodEffect = -10;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() {INTERACTION_TYPE.CLEAN_UP};
        }
    }
}