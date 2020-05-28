using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Fervor : Status {
        private GameObject _fervorGO;

        public Fervor() {
            name = "Fervor";
            description = "This character does not decrease his/her needs.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            moodEffect = 20;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if(addedTo is IPointOfInterest poi) {
                _fervorGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Fervor);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (_fervorGO) {
                ObjectPoolManager.Instance.DestroyObject(_fervorGO);
                _fervorGO = null;
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is IPointOfInterest poi) {
                if (_fervorGO) {
                    ObjectPoolManager.Instance.DestroyObject(_fervorGO);
                    _fervorGO = null;
                }
                _fervorGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Fervor);
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (_fervorGO) {
                ObjectPoolManager.Instance.DestroyObject(_fervorGO);
                _fervorGO = null;
            }
        }
        #endregion
    }
}
