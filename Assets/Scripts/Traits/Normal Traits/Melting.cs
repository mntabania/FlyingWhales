using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Melting : Status {

        public override bool isSingleton => true;

        public Melting() {
            name = "Melting";
            description = "This is melting.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
        }

        #region Overrides
        //public override void OnAddTrait(ITraitable addedTo) {
        //    base.OnAddTrait(addedTo);
        //    if(addedTo is TileObject) {
        //        Messenger.AddListener(Signals.TICK_STARTED, PerTick);
        //    }
        //}
        //public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        //    base.OnRemoveTrait(removedFrom, removedBy);
        //    if (removedFrom is TileObject) {
        //        Messenger.RemoveListener(Signals.TICK_STARTED, PerTick);
        //    }
        //}
        //The reason why there's OnTickStarted and TICK_STARTED listener is because OnTickStarted only works for characters that is if Melting is added to a tile object we add the listener
        public override void OnTickStarted(ITraitable traitable) {
            base.OnTickStarted(traitable);
            PerTick(traitable);
        }
        #endregion

        private void PerTick(ITraitable traitable) {
            if (traitable.gridTileLocation != null) {
                traitable.AdjustHP(-20, ELEMENTAL_TYPE.Normal, true, showHPBar: true);
            }
        }
    }
}
