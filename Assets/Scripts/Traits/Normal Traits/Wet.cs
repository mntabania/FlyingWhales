using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class Wet : Status {

        private StatusIcon _statusIcon;
        public Character dryer { get; private set; }
        
        public Wet() {
            name = "Wet";
            description = "This is soaking wet.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(2); //if this trait is only temporary, then it should not advertise GET_WATER
            isTangible = true;
            isStacking = true;
            moodEffect = -6;
            stackLimit = 10;
            stackModifier = 0f;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EXTRACT_ITEM };
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            addedTo.traitContainer.RemoveTrait(addedTo, "Burning");
            addedTo.traitContainer.RemoveStatusAndStacks(addedTo, "Overheating");
            //if (addedTo is Character character) {
            //    character.needsComponent.AdjustStaminaDecreaseRate(2f);
            //}
            if (addedTo is GenericTileObject genericTileObject) {
                genericTileObject.AddAdvertisedAction(INTERACTION_TYPE.DRY_TILE);
            }
            UpdateVisualsOnAdd(addedTo);
            if (addedTo is DesertRose desertRose) {
                desertRose.DesertRoseEffect();
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
            //if (removedFrom is Character character) {
            //    character.needsComponent.AdjustStaminaDecreaseRate(-2f);
            //}
            if (removedFrom is GenericTileObject genericTileObject) {
                genericTileObject.RemoveAdvertisedAction(INTERACTION_TYPE.DRY_TILE);
            }
            UpdateVisualsOnRemove(removedFrom);
        }
        #endregion

        private void UpdateVisualsOnAdd(ITraitable addedTo) {
            if (addedTo is Character character && _statusIcon == null && character.marker != null) {
                _statusIcon = character.marker.AddStatusIcon(this.name);
            } else if (addedTo is TileObject tileObject) {
                if (tileObject is GenericTileObject) {
                    tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, 
                        InnerMapManager.Instance.assetManager.shoreTile, 0.5f);
                } else if (tileObject.tileObjectType != TILE_OBJECT_TYPE.WATER_WELL && _statusIcon == null){
                    //add water icon above object
                    _statusIcon = addedTo.mapObjectVisual?.AddStatusIcon(this.name);
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
        #endregion

    }
}
