using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Venomous : Trait {
        
        public override bool isSingleton => true;

        public Venomous() {
            name = "Venomous";
            description = "Deals Poison damage. Also sometimes leaks out Poison.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            elementalType = ELEMENTAL_TYPE.Poison;
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            Poisoned poisoned = addedTo.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
            if(poisoned != null) {
                poisoned.SetIsVenomous();
            }
        }
        public override void OnTickStarted(ITraitable traitable) {
            base.OnTickStarted(traitable);
            ApplyPoisonToTile(traitable);
        }
        #endregion

        private void ApplyPoisonToTile(ITraitable traitable) {
            if(traitable.gridTileLocation != null) {
                if (UnityEngine.Random.Range(0, 100) < 10) {
                    traitable.gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.AddTrait(traitable.gridTileLocation.tileObjectComponent.genericTileObject, "Poisoned");
                }
            }
        }
    }
}