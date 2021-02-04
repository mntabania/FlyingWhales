﻿using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = System.Diagnostics.Debug;
namespace Traits {
    public class Poisoned : Status {

        public List<Character> awareCharacters { get; } //characters that know about this trait
        private ITraitable traitable { get; set; } //poi that has the poison
        public Character cleanser { get; private set; }
        public bool isVenomous { get; private set; }
        
        private Character characterOwner;
        private GameObject _poisonedEffect;
        

        #region getters
        public override Type serializedData => typeof(SaveDataPoisoned);
        #endregion
        
        public Poisoned() {
            name = "Poisoned";
            description = "Is suffering from progressive damage. | Is full of poison.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(4);
            isTangible = true;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.REMOVE_POISON, };
            awareCharacters = new List<Character>();
            mutuallyExclusive = new string[] { "Robust" };
            moodEffect = -6;
            isStacking = true;
            stackLimit = 3;
            stackModifier = 0.5f;
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_After_Effect_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataPoisoned saveDataPoisoned = saveDataTrait as SaveDataPoisoned;
            Debug.Assert(saveDataPoisoned != null, nameof(saveDataPoisoned) + " != null");
            isVenomous = saveDataPoisoned.isVenomous;
        }
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
            SaveDataPoisoned saveDataPoisoned = p_saveDataTrait as SaveDataPoisoned;
            Assert.IsNotNull(saveDataPoisoned);
            awareCharacters.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataPoisoned.awareCharacterIDs));
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            traitable = addTo;
            UpdateVisualsOnAdd(addTo);
            if(traitable is Character character) {
                characterOwner = character;
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            traitable = addedTo;
            isVenomous = addedTo.traitContainer.HasTrait("Venomous");
            UpdateVisualsOnAdd(addedTo);
            if(traitable is Character character) {
                characterOwner = character;
                if (!isVenomous) {
                    characterOwner.AdjustDoNotRecoverHP(1);
                }
            } else if (addedTo is TileObject) {
                ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
                if (traitable is GenericTileObject genericTileObject) {
                    genericTileObject.AddAdvertisedAction(INTERACTION_TYPE.CLEANSE_TILE);
                    if (genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Grass || 
                        genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Stone || 
                        genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Sand) {
                        //Reduce duration of poison when put on desert tiles
                        ticksDuration = GameManager.Instance.GetTicksBasedOnHour(2);
                    }
                }
            }
        }
        public override void OnStackStatus(ITraitable addedTo) {
            base.OnStackStatus(addedTo);
            UpdateVisualsOnAdd(addedTo);
        }
        public override void OnStackStatusAddedButStackIsAtLimit(ITraitable traitable) {
            base.OnStackStatusAddedButStackIsAtLimit(traitable);
            UpdateVisualsOnAdd(traitable);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            UpdateVisualsOnRemove(removedFrom);
            if (!isVenomous) {
                characterOwner?.AdjustDoNotRecoverHP(-1);
            }
            if (traitable is GenericTileObject genericTileObject) {
                genericTileObject.RemoveAdvertisedAction(INTERACTION_TYPE.CLEANSE_TILE);
            }
            awareCharacters.Clear();
            responsibleCharacters?.Clear(); //Cleared list, for garbage collection
        }
        public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, ActualGoapNode goapNode, ref bool isRemoved) {
            base.ExecuteActionAfterEffects(action, goapNode, ref isRemoved);
            if (goapNode.action.actionCategory == ACTION_CATEGORY.CONSUME) {
                if(traitable is IPointOfInterest poi && goapNode.poiTarget == poi) {
                    Assert.IsFalse(goapNode.actor == poi, $"Consume action ({goapNode.action.name}) " +
                        $"performed on {goapNode.poiTarget.name} by {goapNode.actor.name} is trying to remove poisoned " +
                        $"stacks from the actor rather than the target!");
                    goapNode.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Ingested_Poison, poi);
                    poi.traitContainer.RemoveStatusAndStacks(poi, this.name);
                    isRemoved = true;
                }
            }
        }
        public override void OnTickStarted(ITraitable traitable1) {
            base.OnTickStarted(traitable);
            if (!isVenomous) {
                characterOwner?.AdjustHP(-Mathf.RoundToInt(1 * characterOwner.traitContainer.stacks[name]),
                ELEMENTAL_TYPE.Normal, true, showHPBar: true);
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is IPointOfInterest poi) {
                if (_poisonedEffect) {
                    ObjectPoolManager.Instance.DestroyObject(_poisonedEffect);
                    _poisonedEffect = null;
                }
                _poisonedEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Poison, false);
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (_poisonedEffect) {
                ObjectPoolManager.Instance.DestroyObject(_poisonedEffect);
                _poisonedEffect = null;
            }
        }
        public override string GetTestingData(ITraitable traitable = null) {
            string data = base.GetTestingData(traitable);
            data += $"\n\tCleanser: {cleanser?.name ?? "None"}";
            data += $"\n\tAware Characters: ";
            for (int i = 0; i < awareCharacters.Count; i++) {
                Character character = awareCharacters[i];
                data += $"{character.name},";    
            }
            return data;
        }
        public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to) {
            base.OnCopyStatus(statusToCopy, from, to);
            if (statusToCopy is Poisoned status) {
                awareCharacters.AddRange(status.awareCharacters);
                cleanser = status.cleanser;
                isVenomous = status.isVenomous;
            }
        }
        #endregion

        #region Aware Characters
        public void AddAwareCharacter(Character character) {
            if (awareCharacters.Contains(character) == false) {
                awareCharacters.Add(character);
                if (traitable is TileObject tileObject) {
                    if(responsibleCharacter != null) {
                        if (character.traitContainer.HasTrait("Cultist") && responsibleCharacter.traitContainer.HasTrait("Cultist") && !tileObject.IsOwnedBy(character)) {
                            //Do not remove poison if both the culprit and the witness are cultists and the poisoned object is not owned by the witness
                        } else {
                            //create remove poison job
                            character.jobComponent.TriggerRemoveStatusTarget(tileObject, "Poisoned");
                        }
                    }
                }
            }
        }
        public void RemoveAwareCharacter(Character character) {
            awareCharacters.Remove(character);
        }
        #endregion

        //This is only called if there is already a Poisoned status before adding the Venomous trait
        public void SetIsVenomous() {
            if (!isVenomous) {
                isVenomous = true;
                characterOwner.AdjustDoNotRecoverHP(1);
            }
        }

        private void UpdateVisualsOnAdd(ITraitable addedTo) {
            if(addedTo is IPointOfInterest pointOfInterest && _poisonedEffect == null && (pointOfInterest is MovingTileObject) == false) {
                _poisonedEffect = GameManager.Instance.CreateParticleEffectAt(pointOfInterest, PARTICLE_EFFECT.Poison, false);
            }
            if (addedTo is TileObject tileObject) {
                if (tileObject is GenericTileObject) {
                    tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, InnerMapManager.Instance.assetManager.poisonRuleTile);
                }
            }
        }
        private void UpdateVisualsOnRemove(ITraitable removedFrom) {
            if(_poisonedEffect) {
                ObjectPoolManager.Instance.DestroyObject(_poisonedEffect);
                _poisonedEffect = null;
            }
            if (removedFrom is TileObject tileObject) {
                if (tileObject is GenericTileObject) {
                    tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, 
                        null);
                }
            }
        }
        
        #region Cleanser
        public void SetCleanser(Character character) {
            cleanser = character;
            if (cleanser == null) {
                Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            } else {
                Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            }
        }
        #endregion
        
        #region Listeners
        private void OnJobRemovedFromCharacter(JobQueueItem jqi, Character character) {
            if (cleanser == character && jqi.jobType == JOB_TYPE.CLEANSE_TILES) {
                SetCleanser(null); 
            }
        }
        #endregion
        
    }
}

#region Save Data
public class SaveDataPoisoned : SaveDataTrait {
    public List<string> awareCharacterIDs;
    public bool isVenomous;

    public override void Save(Trait trait) {
        base.Save(trait);
        Poisoned poisoned = trait as Poisoned;
        Assert.IsNotNull(poisoned);
        awareCharacterIDs = SaveUtilities.ConvertSavableListToIDs(poisoned.awareCharacters);
        isVenomous = poisoned.isVenomous;
    }
}
#endregion