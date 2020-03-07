using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Venomous : Trait {

        private ITraitable _traitable;

        public Venomous() {
            name = "Venomous";
            description = "Damage dealt becomes poison.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            elementalType = ELEMENTAL_TYPE.Poison;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            _traitable = addedTo;
            Poisoned poisoned = addedTo.traitContainer.GetNormalTrait<Poisoned>("Poisoned");
            poisoned.SetIsVenomous();
        }
        public override void OnTickStarted() {
            base.OnTickStarted();
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