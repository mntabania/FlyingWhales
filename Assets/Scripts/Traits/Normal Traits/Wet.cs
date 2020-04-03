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
            if (addedTo is Character character) {
                character.needsComponent.AdjustComfortDecreaseRate(2f);
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
            if (removedFrom is Character character) {
                character.needsComponent.AdjustComfortDecreaseRate(-2f);
            }
            UpdateVisualsOnRemove(removedFrom);
        }
        public override bool IsTangible() {
            return true;
        }
        #endregion

        #region Listeners
        private void OnCharacterChangedState(Character character, CharacterState state) {
            if (state.characterState == CHARACTER_STATE.DRY_TILES && dryer == character) {
                SetDryer(null); 
            }
        }
        #endregion
        
        private void UpdateVisualsOnAdd(ITraitable addedTo) {
            if (addedTo is Character character && _statusIcon == null) {
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
                Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterChangedState);
                Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_PAUSED_STATE, OnCharacterChangedState);
            } else {
                Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterChangedState);
                Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_PAUSED_STATE, OnCharacterChangedState);
            }
        }
        #endregion

    }
}
