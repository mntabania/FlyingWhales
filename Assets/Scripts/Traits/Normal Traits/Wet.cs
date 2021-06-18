using System.Collections;
using System.Collections.Generic;
using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
using Traits;

namespace Traits {
    public class Wet : Status, IElementalTrait {

        private StatusIcon _statusIcon;
        public Character dryer { get; private set; }
        public bool isPlayerSource { get; private set; }

        private ITraitable _owner;

        #region getters
        public override Type serializedData => typeof(SaveDataWet);
        #endregion

        public Wet() {
            name = "Wet";
            description = "Soaked with water.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(6); //if this trait is only temporary, then it should not advertise GET_WATER
            isTangible = true;
            isStacking = true;
            moodEffect = -6;
            stackLimit = 10;
            stackModifier = 0f;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EXTRACT_ITEM };
            AddTraitOverrideFunctionIdentifier(TraitManager.Villager_Reaction);
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataWet data = saveDataTrait as SaveDataWet;
            isPlayerSource = data.isPlayerSource;
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            _owner = addTo;
            TryListenForBiomeEffect();
            UpdateVisualsOnAdd(addTo);
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            _owner = addedTo;
            addedTo.traitContainer.RemoveTrait(addedTo, "Burning");
            addedTo.traitContainer.RemoveStatusAndStacks(addedTo, "Overheating");
            if (addedTo is GenericTileObject genericTileObject) {
                genericTileObject.AddAdvertisedAction(INTERACTION_TYPE.CLEAN_UP);
                if (genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Grass || 
                    genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Stone || 
                    genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Sand) {
                    //Reduce duration of wet when put on desert tiles
                    ticksDuration = GameManager.Instance.GetTicksBasedOnMinutes(30);
                }
            }
            TryListenForBiomeEffect();
            UpdateVisualsOnAdd(addedTo);
            if (addedTo is DesertRose desertRose) {
                desertRose.DesertRoseWaterEffect();
            }
        }
        public override void OnStackStatus(ITraitable addedTo) {
            base.OnStackStatus(addedTo);
            UpdateVisualsOnAdd(addedTo);
        }
        public override void OnStackStatusAddedButStackIsAtLimit(ITraitable addedTo) {
            base.OnStackStatusAddedButStackIsAtLimit(addedTo);
            UpdateVisualsOnAdd(addedTo);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            _owner = null;
            if (removedFrom is GenericTileObject genericTileObject) {
                genericTileObject.RemoveAdvertisedAction(INTERACTION_TYPE.CLEAN_UP);
                Messenger.Broadcast(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR, genericTileObject as TileObject, removedBy);
            }
            StopListenForBiomeEffect();
            UpdateVisualsOnRemove(removedFrom);
        }
        public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to) {
            base.OnCopyStatus(statusToCopy, from, to);
            if (statusToCopy is Wet status) {
                dryer = status.dryer;
            }
        }
        protected override string GetDescriptionInUI() {
            string desc = base.GetDescriptionInUI();
            desc += "\nIs Player Source: " + isPlayerSource;
            return desc;
        }
        #endregion

        #region Visuals
        private void UpdateVisualsOnAdd(ITraitable addedTo) {
            if (addedTo is Character character && _statusIcon == null && character.hasMarker) {
                _statusIcon = character.marker.AddStatusIcon(this.name);
            } else if (addedTo is TileObject tileObject) {
                if (tileObject is GenericTileObject) {
                    tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, InnerMapManager.Instance.assetManager.shoreTile, 0.5f);
                } else if (tileObject.tileObjectType != TILE_OBJECT_TYPE.WATER_WELL && tileObject.tileObjectType != TILE_OBJECT_TYPE.FISHING_SPOT && _statusIcon == null && addedTo.mapObjectVisual != null){
                    //add water icon above object
                    _statusIcon = addedTo.mapObjectVisual.AddStatusIcon(this.name);
                }
            }
        }
        private void UpdateVisualsOnRemove(ITraitable removedFrom) {
            if (removedFrom is Character character && character.hasMarker) {
                ObjectPoolManager.Instance.DestroyObject(_statusIcon.gameObject);
            } else if (removedFrom is TileObject tileObject) {
                if (tileObject is GenericTileObject) {
                    tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, null);
                } else {
                    if (_statusIcon != null) {
                        ObjectPoolManager.Instance.DestroyObject(_statusIcon.gameObject);    
                    }
                }
            }
        }
        #endregion

        #region Dryer
        public void SetDryer(Character character) {
            dryer = character;
            if (dryer == null) {
                Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            } else {
                Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            }
        }
        #endregion
        
        #region Listeners
        private void OnJobRemovedFromCharacter(JobQueueItem jqi, Character character) {
            if (dryer == character && jqi.jobType == JOB_TYPE.DRY_TILES) {
                SetDryer(null); 
            }
        }
        private void TryListenForBiomeEffect() {
            if (_owner.gridTileLocation?.structure is Ocean) {
                return; //do not make ocean frozen if it is part of snow biome
            }
            Messenger.AddListener(AreaSignals.FREEZE_WET_OBJECTS, TryFreezeWetObject);
        }
        private void StopListenForBiomeEffect() {
            Messenger.RemoveListener(AreaSignals.FREEZE_WET_OBJECTS, TryFreezeWetObject);
        }
        private void TryFreezeWetObject() {
            if (GameUtilities.RollChance(25)) {
                if (_owner.gridTileLocation != null && _owner.gridTileLocation.mainBiomeType == BIOMES.SNOW) {
                    _owner.traitContainer.AddTrait(_owner, "Frozen", bypassElementalChance: true);
                }    
            }
        }
        #endregion

        #region IElementalTrait
        public void SetIsPlayerSource(bool p_state) {
            isPlayerSource = p_state;
        }
        #endregion

        #region Reactions
//         public override void VillagerReactionToTileObjectTrait(TileObject owner, Character actor, ref string debugLog) {
//             base.VillagerReactionToTileObjectTrait(owner, actor, ref debugLog);
//             if (!actor.combatComponent.isInActualCombat && !actor.hasSeenWet) {
//                 if (owner.gridTileLocation != null
//                     && actor.homeSettlement != null
//                     && owner.gridTileLocation.IsPartOfSettlement(actor.homeSettlement)
//                     && !actor.jobQueue.HasJob(JOB_TYPE.DRY_TILES)) {
// #if DEBUG_LOG
//                     debugLog = $"{debugLog}\n-Target is Wet";
// #endif
//                     actor.SetHasSeenWet(true);
//                     actor.homeSettlement.settlementJobTriggerComponent.TriggerDryTiles();
//                     for (int i = 0; i < actor.homeSettlement.availableJobs.Count; i++) {
//                         JobQueueItem job = actor.homeSettlement.availableJobs[i];
//                         if (job.jobType == JOB_TYPE.DRY_TILES) {
//                             if (job.assignedCharacter == null && actor.jobQueue.CanJobBeAddedToQueue(job)) {
//                                 actor.jobQueue.AddJobInQueue(job);
//                             }
//                         }
//                     }
//                 }
//             }
//         }
        #endregion
    }
}

#region Save Data
public class SaveDataWet : SaveDataTrait {
    public bool isPlayerSource;

    public override void Save(Trait trait) {
        base.Save(trait);
        Wet data = trait as Wet;
        isPlayerSource = data.isPlayerSource;
    }
}
#endregion