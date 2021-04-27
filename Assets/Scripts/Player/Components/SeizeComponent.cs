using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Ruinarch;
using DG.Tweening;
using Inner_Maps.Location_Structures;

public class SeizeComponent {
    public IPointOfInterest seizedPOI { get; private set; }
    public bool isPreparingToBeUnseized { get; private set; }
    private Sprite _seizedPOISprite;
    private int _seizedPOIVisionTriggerVotes;
    private int _seizedCharacterVisionVotes;
    private bool _seizedPOIVisionTriggerState;
    private Vector3 followOffset;
    private Tween tween;

    #region getters
    public bool hasSeizedPOI => seizedPOI != null;
    #endregion

    public SeizeComponent() {
        followOffset = new Vector3(1f, -1f, 10f); // new Vector3(5, -5, 0f);
    }

    public void SeizePOI(IPointOfInterest poi) {
        if (seizedPOI == null) {
            if (poi.isBeingCarriedBy != null) {
                if (poi.isBeingCarriedBy.carryComponent.isCarryingAnyPOI && poi.isBeingCarriedBy.carryComponent.carriedPOI == poi) {
                    poi.isBeingCarriedBy.UncarryPOI();
                } else {
                    poi.isBeingCarriedBy.UncarryPOI(poi);
                }
            }
            // poi.isBeingCarriedBy?.UncarryPOI();
            if (poi.gridTileLocation != null) {
                Messenger.Broadcast(CharacterSignals.BEFORE_SEIZING_POI, poi);
                seizedPOI = poi;
                _seizedPOISprite = poi.mapObjectVisual.GetSeizeSprite(poi);
                poi.mapObjectVisual.SetVisual(_seizedPOISprite);
                if (poi is BaseMapObject baseMapObject) { baseMapObject.OnManipulatedBy(PlayerManager.Instance.player); }
                _seizedPOIVisionTriggerVotes = poi.mapObjectVisual.visionTrigger.filterVotes;
                // _seizedPOIColor = poi.mapObjectVisual.objectSpriteRenderer.color;    
                if (seizedPOI is Character character) {
                    _seizedCharacterVisionVotes = character.marker.visionCollider.filterVotes;
                }
                _seizedPOIVisionTriggerState = poi.mapObjectVisual.visionTrigger.mainCollider.enabled;
                poi.OnSeizePOI();
                Messenger.Broadcast(CharacterSignals.ON_SEIZE_POI, poi);
                //if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                //} else {
                //    poi.gridTileLocation.structure.RemovePOI(poi);
                //}
            } else {
                Debug.LogError($"Cannot seize. {poi.name} has no tile");
                return;
            }

            PrepareToUnseize();
            Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
            // PlayerManager.Instance.player.AdjustMana(-manaCost);
            //PlayerUI.Instance.ShowSeizedObjectUI();
        } else {
            PlayerUI.Instance.ShowGeneralConfirmation("ERROR", "Already have a seized object. You need to drop the currently seized object first.");
        }
    }
    // public void PrepareToUnseize() {
    //     if (!isPreparingToBeUnseized) {
    //         isPreparingToBeUnseized = true;
    //         CursorManager.Instance.AddLeftClickAction(UnseizePOI);
    //     }
    // }
    private void PrepareToUnseize() {
        isPreparingToBeUnseized = true;
        PlayerManager.Instance.AddPlayerInputModule(PlayerManager.seizeInputModule);
        Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnReceiveKeyCodeSignal);
    }
    private void DoneUnseize() {
        isPreparingToBeUnseized = false;
        PlayerManager.Instance.RemovePlayerInputModule(PlayerManager.seizeInputModule);
        Messenger.RemoveListener<KeyCode>(ControlsSignals.KEY_DOWN, OnReceiveKeyCodeSignal);
    }
    private void OnReceiveKeyCodeSignal(KeyCode keyCode) {
        if (keyCode == KeyCode.Mouse0) {
            TryToUnseize();
        }
    }
    private void TryToUnseize() {
        if (isPreparingToBeUnseized) {
            bool hasUnseized = UnseizePOI();
            if (hasUnseized) {
                DoneUnseize();
            }
        }
    }
    private bool UnseizePOI() {
        if (!CanUnseize()) {
            return false;
        }
        // isPreparingToBeUnseized = false;
        LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
        if (!CanUnseizeHere(hoveredTile)) {
            return false;
        }
        UnseizePOIBase(hoveredTile);
        return true;
    }
    public void UnseizePOIOnDeath() {
        if(seizedPOI != null) {
            UnseizePOIBase(seizedPOI.gridTileLocation);
            DoneUnseize();
        }
    }
    private void UnseizePOIBase(LocationGridTile tileLocation) {
        IPointOfInterest prevSeizedPOI = seizedPOI;
        DisableFollowMousePosition();
        seizedPOI.OnUnseizePOI(tileLocation);
        if (seizedPOI.mapObjectVisual != null) {
            seizedPOI.mapObjectVisual.SetVisual(_seizedPOISprite);
            seizedPOI.mapObjectVisual.visionTrigger.SetFilterVotes(_seizedPOIVisionTriggerVotes);
            seizedPOI.mapObjectVisual.visionTrigger.SetVisionTriggerCollidersState(_seizedPOIVisionTriggerState);
            // seizedPOI.mapObjectVisual.SetColor(_seizedPOIColor);    
        }
        if (seizedPOI is Character character) {
            character.marker.visionCollider.SetFilterVisionVotes(_seizedCharacterVisionVotes);
        }
        _seizedPOISprite = null;
        _seizedPOIVisionTriggerVotes = 0;
        _seizedCharacterVisionVotes = 0;
        _seizedPOIVisionTriggerState = false;
        Messenger.Broadcast(CharacterSignals.ON_UNSEIZE_POI, seizedPOI);
        seizedPOI = null;
        //PlayerUI.Instance.HideSeizedObjectUI();
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        // if (prevSeizedPOI is IPlayerActionTarget playerActionTarget) {
        //     Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, playerActionTarget);
        // }
        Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
    }
    public bool CanUnseize() {
        if (!hasSeizedPOI) {
            //Debug.LogError("Cannot unseize. Not holding seized object");
            return false;
        }
        if (!InnerMapManager.Instance.isAnInnerMapShowing || UIManager.Instance.IsMouseOnUI()) {
            return false;
        }
        //if (seizedPOI.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT && UIManager.Instance.IsMouseOnMapObject()) {
        //    return false;
        //}
        return true;
    }
    public bool CanUnseizeHere(LocationGridTile tileLocation) {
        if(tileLocation == null) {
            return false;
        }
        if (!tileLocation.IsPassable()) {
            return false;
        }
        if (seizedPOI.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            if (tileLocation.tileObjectComponent.objHere != null) {
                return false;
            }
        }
        if (tileLocation.structure is Kennel kennel) {
            if (seizedPOI is Summon summon) {
                return !kennel.HasReachedKennelCapacity() && (kennel.preOccupiedBy == null || kennel.preOccupiedBy == seizedPOI) && (summon.faction == null || !summon.faction.isPlayerFaction);
            }
            return false;
        } else if (tileLocation.structure is TortureChambers prison) {
            if (tileLocation.structure.IsTilePartOfARoom(tileLocation, out var room)) {
                if (seizedPOI is Character character) {
                    return room.CanUnseizeCharacterInRoom(character) && (prison.preOccupiedBy == null || prison.preOccupiedBy == seizedPOI);
                }
                return true;
            } else {
                //in torture chamber, can only unseize inside room
                return false;
            }
        }
        return true;
    }
    private int GetManaCost(IPointOfInterest poi) {
        if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            return 50;
        }
        return 20;
    }

    #region Follow Mouse
    public void EnableFollowMousePosition() {
        if (seizedPOI.visualGO.activeSelf) {
            return;
        }
        seizedPOI.visualGO.transform.position = InnerMapManager.Instance.currentlyShowingMap.worldUiCanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition) + followOffset;
        seizedPOI.visualGO.SetActive(true);
    }
    public void FollowMousePosition() {
        if (!seizedPOI.visualGO.activeSelf) {
            return;
        }
        if (!InnerMapManager.Instance.isAnInnerMapShowing) {
            return;
        }
        Vector3 targetPos = InnerMapManager.Instance.currentlyShowingMap.worldUiCanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition) + followOffset;
        iTween.MoveUpdate(seizedPOI.visualGO, targetPos, 0.5f);
        //seizedPOI.visualGO.transform.domo
    }
    public void DisableFollowMousePosition() {
        if (!seizedPOI.visualGO.activeSelf) {
            return;
        }
        seizedPOI.visualGO.SetActive(false);
        iTween.Stop(seizedPOI.visualGO);
    }
    #endregion
}

[System.Serializable]
public class SaveDataSeizeComponent : SaveData<SeizeComponent> {
    public override void Save(SeizeComponent component) {
    }
    public override SeizeComponent Load() {
        SeizeComponent component = new SeizeComponent();
        return component;
    }
}