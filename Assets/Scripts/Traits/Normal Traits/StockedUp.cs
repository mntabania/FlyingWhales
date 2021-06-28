using System;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class StockedUp : Status {
        
        public StockedUp() {
            name = "Stocked Up";
            description = "Well prepared food-wise.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.POSITIVE;
            ticksDuration = 0;
            moodEffect = 10;
        }
    }
}