using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Traits {
    public class Overheating : Status {
        //public override bool isSingleton => true;
        public ITraitable traitable { get; private set; }
        public List<LocationStructure> excludedStructuresInSeekingShelter { get; private set; }
        public LocationStructure currentShelterStructure { get; private set; }
        private GameObject _overheatingEffectGO;

        public Overheating() {
            name = "Overheating";
            description = "Its temperature is burning up.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(2);
            isStacking = true;
            moodEffect = -6;
            stackLimit = 3;
            stackModifier = 1.5f;
            excludedStructuresInSeekingShelter = new List<LocationStructure>();
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            traitable = addedTo;
            if (addedTo is Character character) {
                Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
                _overheatingEffectGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Overheating);
            }
        }
        public override void OnStackStatus(ITraitable addedTo) {
            base.OnStackStatus(addedTo);
            if (addedTo is Character character) {
                int stacks = character.traitContainer.stacks[name];
                if (stacks >= 1 && stacks < stackLimit && !character.combatComponent.isInCombat) {
                    character.jobComponent.TriggerSeekShelterJob();
                }
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                if (_overheatingEffectGO) {
                    ObjectPoolManager.Instance.DestroyObject(_overheatingEffectGO);
                    _overheatingEffectGO = null;
                }
                if (character.trapStructure.forcedStructure == currentShelterStructure) {
                    character.trapStructure.SetForcedStructure(null);
                }
                Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is Character character) {
                if (_overheatingEffectGO) {
                    ObjectPoolManager.Instance.DestroyObject(_overheatingEffectGO);
                    _overheatingEffectGO = null;
                }
                _overheatingEffectGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Overheating);
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (_overheatingEffectGO) {
                ObjectPoolManager.Instance.DestroyObject(_overheatingEffectGO);
                _overheatingEffectGO = null;
            }
        }
        #endregion

        #region General
        public void AddExcludedStructureInSeekingShelter(LocationStructure structure) {
            excludedStructuresInSeekingShelter.Add(structure);
        }
        public bool IsStructureExludedInSeekingShelter(LocationStructure structure) {
            return excludedStructuresInSeekingShelter.Contains(structure);
        }
        public void SetCurrentShelterStructure(LocationStructure structure) {
            currentShelterStructure = structure;
        }
        private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
            if (traitable is Character owner && owner == character) {
                if (currentShelterStructure != null && currentShelterStructure != structure) {
                    AddExcludedStructureInSeekingShelter(currentShelterStructure);
                    SetCurrentShelterStructure(null);
                    owner.trapStructure.SetForcedStructure(null);
                }
            }
        }
        #endregion
    }
}
