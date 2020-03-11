using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class DefaultTraitProcessor : TraitProcessor {
        public override void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible, ActualGoapNode gainedFromDoing, int overrideDuration) {
            DefaultProcessOnAddTrait(traitable, trait, characterResponsible, gainedFromDoing, overrideDuration);
        }
        public override void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy) {
            DefaultProcessOnRemoveTrait(traitable, trait, removedBy);
        }
        public override void OnStatusStacked(ITraitable traitable, Status status, Character characterResponsible, ActualGoapNode gainedFromDoing, int overrideDuration) {
            DefaultProcessOnStackStatus(traitable, status, characterResponsible, gainedFromDoing, overrideDuration);
        }
        public override void OnStatusUnstack(ITraitable traitable, Status status, Character removedBy = null) {
            DefaultProcessOnUnstackStatus(traitable, status, removedBy);
        }
    }

}
