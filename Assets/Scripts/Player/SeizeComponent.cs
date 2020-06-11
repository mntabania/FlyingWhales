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
    private int _seizedPOIVisionVotes;

    private Vector3 followOffset;
    private Tween tween;

    #region getters
    public bool hasSeizedPOI => seizedPOI != null;
    #endregion

    public SeizeComponent() {
        followOffset = new Vector3(1f, -1f, 10f); // new Vector3(5, -5, 0f);
    }

    public void SeizePOI(IPointOfInterest poi) {
        // int manaCost = GetManaCost(poi);
        // if (PlayerManager.Instance.player.mana < manaCost) {
        //     PlayerUI.Instance.ShowGeneralConfirmation("ERROR", "Not enough mana! You need " + manaCost + " mana to seize this object.");
        //     return;
        // }
        if (seizedPOI == null) {
            poi.isBeingCarriedBy?.UncarryPOI();
            if (poi.gridTileLocation != null) {
                Messenger.Broadcast(Signals.BEFORE_SEIZING_POI, poi);
                _seizedPOISprite = poi.mapObjectVisual.usedSprite;
                if (poi is BaseMapObject baseMapObject) { baseMapObject.OnManipulatedBy(PlayerManager.Instance.player); }
                _seizedPOIVisionVotes = poi.mapObjectVisual.visionTrigger.filterVotes;
                poi.OnSeizePOI();
                Messenger.Broadcast(Signals.ON_SEIZE_POI, poi);
                //if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                //} else {
                //    poi.gridTileLocation.structure.RemovePOI(poi);
                //}
            } else {
                Debug.LogError($"Cannot seize. {poi.name} has no tile");
                return;
            }
            seizedPOI = poi;
            PrepareToUnseize();
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
        Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnReceiveKeyCodeSignal);
    }
    private void DoneUnseize() {
        isPreparingToBeUnseized = false;
        Messenger.RemoveListener<KeyCode>(Signals.KEY_DOWN, OnReceiveKeyCodeSignal);
    }
    private void OnReceiveKeyCodeSignal(KeyCode keyCode) {
        if(keyCode == KeyCode.Mouse0) {
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
        IPointOfInterest prevSeizedPOI = seizedPOI;
        LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
        if (!CanUnseizeHere(hoveredTile)) {
            return false;
        }
        DisableFollowMousePosition();
        seizedPOI.OnUnseizePOI(hoveredTile);
        if (seizedPOI.mapObjectVisual != null) {
            seizedPOI.mapObjectVisual.SetVisual(_seizedPOISprite);    
            seizedPOI.mapObjectVisual.visionTrigger.SetFilterVotes(_seizedPOIVisionVotes);
        }
        _seizedPOISprite = null;
        _seizedPOIVisionVotes = 0;
        Messenger.Broadcast(Signals.ON_UNSEIZE_POI, seizedPOI);
        seizedPOI = null;
        //PlayerUI.Instance.HideSeizedObjectUI();
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        if (prevSeizedPOI is IPlayerActionTarget playerActionTarget) {
            Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, playerActionTarget);
        }
        return true;
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
        if (seizedPOI.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            if (tileLocation.objHere != null) {
                return false;
            }
        } else if (seizedPOI.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            if (!tileLocation.IsPassable()) {
                return false;
            }
        }
        if (tileLocation.structure.structureType == STRUCTURE_TYPE.KENNEL) {
            if (seizedPOI is Summon summon) {
                int numOfSummons = tileLocation.structure.GetNumberOfSummonsHere();
                if(numOfSummons < 3) {
                    return true;
                }
            }
            return false;
        } else if (tileLocation.structure.structureType == STRUCTURE_TYPE.CRYPT) {
            if(seizedPOI is Tombstone) {
                //Temporarily disabled unseizing tombstones in crypt because we still do not have a save data for characters, hence, we cannot save tombstones
                return false;
            }
            if (seizedPOI is TileObject tileObject) {
                int numOfTileOjects = tileLocation.structure.GetNumberOfNonPreplacedTileObjectsHere();
                if (numOfTileOjects < 3) {
                    return true;
                }
            }
            return false;
        } else if (tileLocation.structure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS) {
            if (tileLocation.structure.IsTilePartOfARoom(tileLocation, out var room)) {
                if (room is TortureRoom tortureRoom && seizedPOI is Character character) {
                    return tortureRoom.CanUnseizeCharacterInRoom(character);
                }
            }
            return true;
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
