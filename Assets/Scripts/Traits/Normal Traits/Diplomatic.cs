using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Diplomatic : Trait {
        public override bool isSingleton => true;

        public Diplomatic() {
            name = "Diplomatic";
            description = "A typical peaceloving do-gooder. Can mend relationships.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        #region Overrides
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character targetCharacter) {
                if(targetCharacter.limiterComponent.canPerform) {
                    int chance = UnityEngine.Random.Range(0, 100);
                    if (chance < 4) {
                        if (targetCharacter.relationshipContainer.HasEnemyCharacter() && !characterThatWillDoJob.relationshipContainer.IsEnemiesWith(targetCharacter)) {
                            characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Reduce_Conflict, targetCharacter);
                        }
                    }
                }
                
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        #endregion
    }
}

