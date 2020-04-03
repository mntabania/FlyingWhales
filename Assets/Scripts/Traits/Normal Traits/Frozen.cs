using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Frozen : Status {
        private GameObject _frozenEffect;

        public Frozen() {
            name = "Frozen";
            description = "This is frozen.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(4);
            isStacking = true;
            moodEffect = -5;
            stackLimit = 1;
            stackModifier = 1f;
            hindersMovement = true;
            hindersPerform = true;
            hindersWitness = true;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EXTRACT_ITEM, INTERACTION_TYPE.REMOVE_FREEZING };
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if(addedTo is IPointOfInterest poi) {
                _frozenEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Frozen, false);
            }
            if (addedTo is Character) {
                Character character = addedTo as Character;
                character.needsComponent.AdjustDoNotGetBored(1);
                character.needsComponent.AdjustDoNotGetHungry(1);
                character.needsComponent.AdjustDoNotGetTired(1);
                character.needsComponent.AdjustDoNotGetUncomfortable(1);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if(_frozenEffect) {
                ObjectPoolManager.Instance.DestroyObject(_frozenEffect);
                _frozenEffect = null;
            }
            if (removedFrom is Character) {
                Character character = removedFrom as Character;
                character.needsComponent.AdjustDoNotGetBored(-1);
                character.needsComponent.AdjustDoNotGetHungry(-1);
                character.needsComponent.AdjustDoNotGetTired(-1);
                character.needsComponent.AdjustDoNotGetUncomfortable(-1);
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is IPointOfInterest poi) {
                if (_frozenEffect) {
                    ObjectPoolManager.Instance.DestroyObject(_frozenEffect);
                    _frozenEffect = null;
                }
                _frozenEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Frozen, false);
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (_frozenEffect) {
                ObjectPoolManager.Instance.DestroyObject(_frozenEffect);
                _frozenEffect = null;
            }
        }
        #endregion
    }
}
