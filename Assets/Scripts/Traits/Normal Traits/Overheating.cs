using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class Overheating : Status {
        
        public ITraitable traitable { get; private set; }
        public List<LocationStructure> excludedStructuresInSeekingShelter { get; private set; }
        public LocationStructure currentShelterStructure { get; private set; }
        
        private GameObject _overheatingEffectGO;
        private readonly WeightedDictionary<string> weights;

        #region getters
        public override Type serializedData => typeof(SaveDataOverheating);
        #endregion
        
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
            weights = new WeightedDictionary<string>();
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataOverheating saveDataOverheating = saveDataTrait as SaveDataOverheating;
            Assert.IsNotNull(saveDataOverheating);
            excludedStructuresInSeekingShelter = SaveUtilities.ConvertIDListToStructures(saveDataOverheating.excludedStructuresInSeekingShelter);
            if (!string.IsNullOrEmpty(saveDataOverheating.currentShelterStructure)) {
                currentShelterStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataOverheating.currentShelterStructure);    
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            traitable = addTo;
            if (addTo is Character character) {
                Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
                _overheatingEffectGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Overheating);
            }
        }
        #endregion
        
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
        public override bool PerTickOwnerMovement() {
            int roll = UnityEngine.Random.Range(0, 1000);
            int chance = 15 * traitable.traitContainer.GetStacks(name);
            if (roll < chance) {
                return OverheatingEffects();
            }
            return false;
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
        private bool OverheatingEffects() {
            if(traitable is Character character) {
                if (!character.isDead) {
                    weights.Clear();
                    if (!character.traitContainer.HasTrait("Unconscious")) {
                        weights.AddElement("unconscious", 20);
                    }
                    weights.AddElement("heatstroke", 20);
                    weights.AddElement("seizure", 20);

                    string result = weights.PickRandomElementGivenWeights();
                    if(result == "unconscious") {
                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "overheat_unconscious");
                        log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddLogToDatabase();

                        character.traitContainer.AddTrait(character, "Unconscious");
                        return true;
                    } else if (result == "heatstroke") {
                        return character.interruptComponent.TriggerInterrupt(INTERRUPT.Heatstroke_Death, character);
                    } else if (result == "seizure") {
                        return character.interruptComponent.TriggerInterrupt(INTERRUPT.Seizure, character);
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
#region Save Data
public class SaveDataOverheating : SaveDataTrait {
    public List<string> excludedStructuresInSeekingShelter;
    public string currentShelterStructure;
    public override void Save(Trait trait) {
        base.Save(trait);
        Overheating overheating = trait as Overheating;
        Assert.IsNotNull(overheating);
        excludedStructuresInSeekingShelter = SaveUtilities.ConvertSavableListToIDs(overheating.excludedStructuresInSeekingShelter);
        if (overheating.currentShelterStructure != null) {
            currentShelterStructure = overheating.currentShelterStructure.persistentID;    
        }
    }
}
#endregion
