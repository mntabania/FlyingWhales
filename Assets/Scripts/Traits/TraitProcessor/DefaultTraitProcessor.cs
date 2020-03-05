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
        public override void OnTraitStacked(ITraitable traitable, Trait trait, Character characterResponsible, ActualGoapNode gainedFromDoing, int overrideDuration) {
            DefaultProcessOnStackTrait(traitable, trait, characterResponsible, gainedFromDoing, overrideDuration);
        }
        public override void OnTraitUnstack(ITraitable traitable, Trait trait, Character removedBy = null) {
            DefaultProcessOnUnstackTrait(traitable, trait, removedBy);
        }
    }

}
