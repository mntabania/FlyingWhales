using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
namespace Traits {
    public class BurningAtStake : Status {
        private Character owner { get; set; }
        private GameObject burningEffect;

        public BurningAtStake() {
            name = "Burning At Stake";
            description = "Burning to death!";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            isHidden = true;
            hindersSocials = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
        }

        #region Loading
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if(addTo is Character character) {
                owner = character;
                burningEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Burning, false);
                if (owner.marker) {
                    owner.marker.ShowAdditionalEffect(CharacterManager.Instance.stakeEffect);
                }
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            if (addedTo is Character character) {
                owner = character;
                burningEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Burning, false);
                if (owner.marker) {
                    owner.marker.ShowAdditionalEffect(CharacterManager.Instance.stakeEffect);
                }
            }
            base.OnAddTrait(addedTo);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (burningEffect) {
                ObjectPoolManager.Instance.DestroyObject(burningEffect);
                burningEffect = null;
            }
            if (owner.marker) {
                if (owner.marker.IsShowingAdditionEffectImage(CharacterManager.Instance.stakeEffect)) {
                    owner.marker.HideAdditionalEffect();
                }
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (burningEffect) {
                ObjectPoolManager.Instance.DestroyObject(burningEffect);
                burningEffect = null;
            }
            if (traitable is Character character) {
                burningEffect = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Burning, false);
                if (character.marker) {
                    character.marker.ShowAdditionalEffect(CharacterManager.Instance.stakeEffect);
                }
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (burningEffect) {
                ObjectPoolManager.Instance.DestroyObject(burningEffect);
                burningEffect = null;
            }
            if (traitable is Character character) {
                if (character.marker) {
                    if (character.marker.IsShowingAdditionEffectImage(CharacterManager.Instance.stakeEffect)) {
                        character.marker.HideAdditionalEffect();
                    }
                }
            }
        }
        #endregion
    }
}