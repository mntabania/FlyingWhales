﻿using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class Freezing : Status {

        public ITraitable traitable { get; private set; }
        private GameObject _freezingGO;
        public List<LocationStructure> excludedStructuresInSeekingShelter { get; private set; }
        public LocationStructure currentShelterStructure { get; private set; }

        #region getters
        public override Type serializedData => typeof(SaveDataFreezing);
        #endregion
        
        public Freezing() {
            name = "Freezing";
            description = "May be completely Frozen soon.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(1);
            isStacking = true;
            moodEffect = -5;
            stackLimit = 3;
            stackModifier = 1f;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.REMOVE_FREEZING, INTERACTION_TYPE.TAKE_SHELTER };
            excludedStructuresInSeekingShelter = new List<LocationStructure>();
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
        }

        #region Loading
        public override void LoadInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadInstancedTrait(saveDataTrait);
            SaveDataFreezing saveDataFreezing = saveDataTrait as SaveDataFreezing;
            Assert.IsNotNull(saveDataFreezing);
            excludedStructuresInSeekingShelter = SaveUtilities.ConvertIDListToStructures(saveDataFreezing.excludedStructuresInSeekingShelter);
            if (!string.IsNullOrEmpty(saveDataFreezing.currentShelterStructure)) {
                currentShelterStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataFreezing.currentShelterStructure);    
            }
        }
        #endregion

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            traitable = addedTo;
            if (addedTo is Character character) {
                _freezingGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Freezing);
                //character.needsComponent.AdjustStaminaDecreaseRate(1f);
                character.needsComponent.AdjustTirednessDecreaseRate(1f);
                character.movementComponent.AdjustSpeedModifier(-0.15f);
                Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            } else if (addedTo is IPointOfInterest poi) {
                _freezingGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Freezing_Object);
            }
            
            if (addedTo.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Grass || 
                addedTo.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Stone || 
                addedTo.gridTileLocation.groundType == LocationGridTile.Ground_Type.Sand) {
                //Desert Biomes should immediately remove freezing and frozen status
                ticksDuration = GameManager.Instance.GetTicksBasedOnMinutes(5);
            }
        }
        public override void OnStackStatus(ITraitable addedTo) {
            base.OnStackStatus(addedTo);
            if (addedTo is Character) {
                Character character = addedTo as Character;
                character.movementComponent.AdjustSpeedModifier(-0.15f);
                int stacks = character.traitContainer.stacks[name];
                if (stacks >= 1 && stacks < stackLimit && !character.combatComponent.isInCombat) {
                    character.jobComponent.TriggerSeekShelterJob();
                }
            }
        }
        public override void OnUnstackStatus(ITraitable addedTo) {
            base.OnUnstackStatus(addedTo);
            if (addedTo is Character) {
                Character character = addedTo as Character;
                character.movementComponent.AdjustSpeedModifier(0.15f);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (_freezingGO) {
                ObjectPoolManager.Instance.DestroyObject(_freezingGO);
                _freezingGO = null;
            }
            if (removedFrom is Character character) {
                //character.needsComponent.AdjustStaminaDecreaseRate(-1f);
                character.needsComponent.AdjustTirednessDecreaseRate(-1f);
                character.movementComponent.AdjustSpeedModifier(0.15f);
                if(character.trapStructure.forcedStructure == currentShelterStructure) {
                    character.trapStructure.SetForcedStructure(null);
                }
                Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is IPointOfInterest poi) {
                if (_freezingGO) {
                    ObjectPoolManager.Instance.DestroyObject(_freezingGO);
                    _freezingGO = null;
                }
                PARTICLE_EFFECT particleEffect = PARTICLE_EFFECT.Freezing_Object;
                if(poi is Character) {
                    particleEffect = PARTICLE_EFFECT.Freezing;
                }
                _freezingGO = GameManager.Instance.CreateParticleEffectAt(poi, particleEffect);
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (_freezingGO) {
                ObjectPoolManager.Instance.DestroyObject(_freezingGO);
                _freezingGO = null;
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
            if(traitable is Character owner && owner == character) {
                if(currentShelterStructure != null && currentShelterStructure != structure) {
                    AddExcludedStructureInSeekingShelter(currentShelterStructure);
                    SetCurrentShelterStructure(null);
                    owner.trapStructure.SetForcedStructure(null);
                }
            }
        }
        #endregion
    }
}

#region Save Data
public class SaveDataFreezing : SaveDataTrait {
    public List<string> excludedStructuresInSeekingShelter;
    public string currentShelterStructure;
    public override void Save(Trait trait) {
        base.Save(trait);
        Freezing freezing = trait as Freezing;
        Assert.IsNotNull(freezing);
        excludedStructuresInSeekingShelter = SaveUtilities.ConvertSavableListToIDs(freezing.excludedStructuresInSeekingShelter);
        if (freezing.currentShelterStructure != null) {
            currentShelterStructure = freezing.currentShelterStructure.persistentID;    
        }
    }
}
#endregion
