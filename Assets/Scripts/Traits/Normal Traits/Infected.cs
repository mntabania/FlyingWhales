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
            //isHidden = true;
            //AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        //#region Override
        //public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        //    if (characterThatWillDoJob.limiterComponent.canPerform && characterThatWillDoJob.limiterComponent.canMove && !characterThatWillDoJob.isDead) {
        //        if (targetPOI.traitContainer.HasTrait("Edible")) {
        //            characterThatWillDoJob.jobComponent.CreateEatJob(targetPOI);
        //        }
        //    }
        //    return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        //}
        //#endregion
    }
}