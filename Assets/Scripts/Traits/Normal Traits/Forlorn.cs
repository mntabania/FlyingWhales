using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Forlorn : Trait {

        public ForlornSpirit owner { get; private set; }
        public Forlorn() {
            name = "Forlorn";
            description = "This is forlorn.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
            //hasOnCollideWith = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Collision_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is ForlornSpirit) {
                owner = addedTo as ForlornSpirit;
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
