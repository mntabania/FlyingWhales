using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Ravenous : Trait {

        public RavenousSpirit owner { get; private set; }
        public Ravenous() {
            name = "Ravenous";
            description = "This is ravenous.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
            hasOnCollideWith = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is RavenousSpirit) {
                owner = addedTo as RavenousSpirit;
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
