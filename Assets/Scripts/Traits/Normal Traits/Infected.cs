using System;
using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
namespace Traits {
    public class Infected : Trait {

        #region getters
        public override bool isSingleton => true;
        #endregion
        
        public Infected() {
            name = "Infected";
            description = "Will eat anything edible on sight.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            moodEffect = 0;
        }
    }
}