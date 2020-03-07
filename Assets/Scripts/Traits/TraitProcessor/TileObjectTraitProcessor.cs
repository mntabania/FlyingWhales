using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class TileObjectTraitProcessor : TraitProcessor {
        public override void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible, ActualGoapNode gainedFromDoing, int overrideDuration) {
            TileObject obj = traitable as TileObject;
            obj.OnTileObjectGainedTrait(trait);
            DefaultProcessOnAddTrait(traitable, trait, characterResponsible, gainedFromDoing, overrideDuration);
            Messenger.Broadcast(Signals.TILE_OBJECT_TRAIT_ADDED, obj, trait);
        }
        public override void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy) {
            TileObject obj = traitable as TileObject;
            DefaultProcessOnRemoveTrait(traitable, trait, removedBy);
            obj.OnTileObjectLostTrait(trait);
            Messenger.Broadcast(Signals.TILE_OBJECT_TRAIT_REMOVED, obj, trait);
        }
        public override void OnStatusStacked(ITraitable traitable, Status status, Character characterResponsible, ActualGoapNode gainedFromDoing, int overrideDuration) {
            if(DefaultProcessOnStackStatus(traitable, status, characterResponsible, gainedFromDoing, overrideDuration)) {
                Messenger.Broadcast(Signals.TILE_OBJECT_TRAIT_STACKED, traitable as TileObject, status.GetBase());
            }
        }
        public override void OnStatusUnstack(ITraitable traitable, Status status, Character removedBy = null) {
            DefaultProcessOnUnstackStatus(traitable, status, removedBy);
            Messenger.Broadcast(Signals.TILE_OBJECT_TRAIT_UNSTACKED, traitable as TileObject, status.GetBase()); 
        }
    }

}
