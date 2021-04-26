using System.Collections;
using System.Collections.Generic;
using System;
using Inner_Maps;
using UnityEngine;
using Traits;

namespace Traits {
    public class Frozen : Status, IElementalTrait {
        private GameObject _frozenEffect;
        public ITraitable traitable { get; private set; }
        public bool isPlayerSource { get; private set; }

        #region getters
        public override Type serializedData => typeof(SaveDataFrozen);
        #endregion

        public Frozen() {
            name = "Frozen";
            description = "Encased in ice.";
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

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataFrozen data = saveDataTrait as SaveDataFrozen;
            isPlayerSource = data.isPlayerSource;
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            traitable = addTo;
            if (addTo.gridTileLocation == null) {
                return;
            }
            if(addTo is IPointOfInterest poi && poi is GenericTileObject == false) {
                _frozenEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Frozen, false);
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            traitable = addedTo;
            if (addedTo.gridTileLocation == null) {
                return;
            }
            if(addedTo is IPointOfInterest poi && poi is GenericTileObject == false) {
                _frozenEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Frozen, false);
            }
            if (addedTo is Character) {
                Character character = addedTo as Character;
                character.needsComponent.AdjustDoNotGetBored(1);
                character.needsComponent.AdjustDoNotGetHungry(1);
                character.needsComponent.AdjustDoNotGetTired(1);
                character.needsComponent.AdjustDoNotGetDrained(1);
            }
            if (addedTo.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Grass || 
                addedTo.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Stone || 
                addedTo.gridTileLocation.groundType == LocationGridTile.Ground_Type.Sand) {
                //Desert Biomes should immediately remove freezing and frozen status
                ticksDuration = GameManager.Instance.GetTicksBasedOnMinutes(3);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            traitable = null;
            if (_frozenEffect) {
                ObjectPoolManager.Instance.DestroyObject(_frozenEffect);
                _frozenEffect = null;
            }
            if (removedFrom is Character) {
                Character character = removedFrom as Character;
                character.needsComponent.AdjustDoNotGetBored(-1);
                character.needsComponent.AdjustDoNotGetHungry(-1);
                character.needsComponent.AdjustDoNotGetTired(-1);
                character.needsComponent.AdjustDoNotGetDrained(-1);
                DisablePlayerSourceChaosOrb(character);
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is IPointOfInterest poi) {
                if (_frozenEffect) {
                    ObjectPoolManager.Instance.DestroyObject(_frozenEffect);
                    _frozenEffect = null;
                }
                if (poi is GenericTileObject == false) {
                    _frozenEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Frozen, false);    
                }
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (_frozenEffect) {
                ObjectPoolManager.Instance.DestroyObject(_frozenEffect);
                _frozenEffect = null;
            }
        }
        protected override string GetDescriptionInUI() {
            string desc = base.GetDescriptionInUI();
            desc += "\nIs Player Source: " + isPlayerSource;
            return desc;
        }
        #endregion

        #region IElementalTrait
        public void SetIsPlayerSource(bool p_state) {
            if (isPlayerSource != p_state) {
                isPlayerSource = p_state;
                if (traitable is Character character) {
                    if (isPlayerSource) {
                        EnablePlayerSourceChaosOrb(character);
                    } else {
                        DisablePlayerSourceChaosOrb(character);
                    }
                }
            }
        }
        #endregion
    }
}
#region Save Data
public class SaveDataFrozen : SaveDataTrait {
    public bool isPlayerSource;

    public override void Save(Trait trait) {
        base.Save(trait);
        Frozen data = trait as Frozen;
        isPlayerSource = data.isPlayerSource;
    }
}
#endregion
