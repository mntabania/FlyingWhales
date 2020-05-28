using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Disoriented : Status {

        public Disoriented() {
            name = "Disoriented";
            description = "This is disoriented.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnMinutes(30);
            moodEffect = -5;
            hindersMovement = true;
            hindersPerform = true;
        }

        // #region Overrides
        // public override void OnAddTrait(ITraitable addedTo) {
        //     base.OnAddTrait(addedTo);
        //     if (addedTo is Character) {
        //         Character character = addedTo as Character;
        //         character.needsComponent.AdjustDoNotGetBored(1);
        //         character.needsComponent.AdjustDoNotGetHungry(1);
        //         character.needsComponent.AdjustDoNotGetTired(1);
        //         character.needsComponent.AdjustDoNotGetUncomfortable(1);
        //     }
        // }
        // public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        //     base.OnRemoveTrait(removedFrom, removedBy);
        //     if (removedFrom is Character) {
        //         Character character = removedFrom as Character;
        //         character.needsComponent.AdjustDoNotGetBored(-1);
        //         character.needsComponent.AdjustDoNotGetHungry(-1);
        //         character.needsComponent.AdjustDoNotGetTired(-1);
        //         character.needsComponent.AdjustDoNotGetUncomfortable(-1);
        //     }
        // }
        // #endregion
    }
}