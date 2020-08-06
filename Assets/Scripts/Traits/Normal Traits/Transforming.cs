using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Traits {
    public class Transforming : Status {
        private GameObject _transformRevertEffectGO;

        public Transforming() {
            name = "Transforming";
            description = "Transforming";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                _transformRevertEffectGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Transform_Revert, false);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                if (_transformRevertEffectGO) {
                    ObjectPoolManager.Instance.DestroyObject(_transformRevertEffectGO);
                    _transformRevertEffectGO = null;
                }
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is Character character) {
                if (_transformRevertEffectGO) {
                    ObjectPoolManager.Instance.DestroyObject(_transformRevertEffectGO);
                    _transformRevertEffectGO = null;
                }
                _transformRevertEffectGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Transform_Revert, false);
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (_transformRevertEffectGO) {
                ObjectPoolManager.Instance.DestroyObject(_transformRevertEffectGO);
                _transformRevertEffectGO = null;
            }
        }
        #endregion
    }
}
