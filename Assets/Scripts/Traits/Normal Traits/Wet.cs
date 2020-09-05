using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Traits {
    public class Wet : Status {

        private StatusIcon _statusIcon;
        public Character dryer { get; private set; }
        private ITraitable _owner;
        
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
        }

        #region Loading
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
                genericTileObject.AddAdvertisedAction(INTERACTION_TYPE.DRY_TILE);
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
                genericTileObject.RemoveAdvertisedAction(INTERACTION_TYPE.DRY_TILE);
                Messenger.Broadcast(Signals.STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR, genericTileObject as IPointOfInterest, removedBy);
            }
            StopListenForBiomeEffect();
            UpdateVisualsOnRemove(removedFrom);
        }
        #endregion

        #region Visuals
        private void UpdateVisualsOnAdd(ITraitable addedTo) {
            if (addedTo is Character character && _statusIcon == null && character.marker != null) {
                _statusIcon = character.marker.AddStatusIcon(this.name);
            } else if (addedTo is TileObject tileObject) {
                if (tileObject is GenericTileObject) {
                    tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, 
                        InnerMapManager.Instance.assetManager.shoreTile, 0.5f);
                } else if (tileObject.tileObjectType != TILE_OBJECT_TYPE.WATER_WELL && _statusIcon == null && addedTo.mapObjectVisual != null){
                    //add water icon above object
                    _statusIcon = addedTo.mapObjectVisual.AddStatusIcon(this.name);
                }
            }
        }
        private void UpdateVisualsOnRemove(ITraitable removedFrom) {
            if (removedFrom is Character) {
                ObjectPoolManager.Instance.DestroyObject(_statusIcon.gameObject);
            } else if (removedFrom is TileObject tileObject) {
                if (tileObject is GenericTileObject) {
                    tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, 
                        null);
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
                Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            } else {
                Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
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
            Messenger.AddListener<HexTile>(Signals.FREEZE_WET_OBJECTS_IN_TILE, TryFreezeWetObject);
        }
        private void StopListenForBiomeEffect() {
            Messenger.RemoveListener<HexTile>(Signals.FREEZE_WET_OBJECTS_IN_TILE, TryFreezeWetObject);
        }
        private void TryFreezeWetObject(HexTile hexTile) {
            if (GameUtilities.RollChance(25)) {
                if (_owner.gridTileLocation != null && _owner.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                    if (_owner.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == hexTile) {
                        _owner.traitContainer.AddTrait(_owner, "Frozen", bypassElementalChance: true);
                    }
                }    
            }
        }
        #endregion

    }
}
