using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Traits {
    public class Stoned : Status {

        private GameObject _stonedGO;

        public Stoned() {
            name = "Stoned";
            description = "This has been turned into a stone.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            hindersMovement = true;
            hindersPerform = true;
            hindersWitness = true;
            hindersAttackTarget = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
        }

        #region Loading
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if(addTo is Character character && character.marker) {
                character.marker.PauseAnimation();
                _stonedGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Stoned);
                _stonedGO.GetComponent<StonedEffect>().PlayEffect(character.marker.usedSprite);
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if(addedTo is Character character && character.marker) {
                character.marker.PauseAnimation();
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", name, "effect", null, LOG_TAG.Life_Changes);
                log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase(true);
                _stonedGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Stoned);
                _stonedGO.GetComponent<StonedEffect>().PlayEffect(character.marker.usedSprite);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (_stonedGO) {
                ObjectPoolManager.Instance.DestroyObject(_stonedGO);
                _stonedGO = null;
            }
            if (removedFrom is Character character && character.marker) {
                character.marker.UnpauseAnimation();
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is Character character) {
                if (_stonedGO) {
                    ObjectPoolManager.Instance.DestroyObject(_stonedGO);
                    _stonedGO = null;
                }
                _stonedGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Stoned);
                _stonedGO.GetComponent<StonedEffect>().PlayEffect(character.marker.usedSprite);
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (_stonedGO) {
                ObjectPoolManager.Instance.DestroyObject(_stonedGO);
                _stonedGO = null;
            }
        }
        public override bool OnDeath(Character character) {
            return character.traitContainer.RemoveTrait(character, this);
        }
        #endregion
    }
}
