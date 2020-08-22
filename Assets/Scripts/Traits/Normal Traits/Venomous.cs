using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Venomous : Trait {

        private ITraitable _traitable;

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
            _traitable = addedTo;
            Poisoned poisoned = addedTo.traitContainer.GetNormalTrait<Poisoned>("Poisoned");
            if(poisoned != null) {
                poisoned.SetIsVenomous();
            }
        }
        public override void OnTickStarted(ITraitable traitable) {
            base.OnTickStarted(traitable);
            ApplyPoisonToTile();
        }
        #endregion

        private void ApplyPoisonToTile() {
            if(_traitable.gridTileLocation != null) {
                if (UnityEngine.Random.Range(0, 100) < 10) {
                    _traitable.gridTileLocation.genericTileObject.traitContainer.AddTrait(_traitable.gridTileLocation.genericTileObject, "Poisoned");
                }
            }
        }
    }
}