using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Feeble : Trait {

        public FeebleSpirit owner { get; private set; }
        public Feeble() {
            name = "Feeble";
            description = "This is feeble.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
            hasOnCollideWith = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is FeebleSpirit) {
                owner = addedTo as FeebleSpirit;
            }
        }
        public override bool OnCollideWith(IPointOfInterest collidedWith, IPointOfInterest owner) {
            if (collidedWith is Character) {
                Character target = collidedWith as Character;
                if (target.needsComponent.HasNeeds()) {
                    this.owner.StartSpiritPossession(target);
                }
            }
            return true;       
        }
        #endregion
    }
}
