using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Melting : Status {

        public ITraitable traitable { get; private set; }

        public Melting() {
            name = "Melting";
            description = "This is melting.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            traitable = addedTo;
            if(traitable is TileObject) {
                Messenger.AddListener(Signals.TICK_STARTED, PerTick);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (traitable is TileObject) {
                Messenger.RemoveListener(Signals.TICK_STARTED, PerTick);
            }
        }
        //The reason why there's OnTickStarted and TICK_STARTED listener is because OnTickStarted only works for characters that is if Melting is added to a tile object we add the listener
        public override void OnTickStarted() {
            base.OnTickStarted();
            PerTick();
        }
        #endregion

        private void PerTick() {
            if (traitable.gridTileLocation != null) {
                traitable.AdjustHP(-50, ELEMENTAL_TYPE.Normal, true, showHPBar: true);
            }
        }
    }
}
